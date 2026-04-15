using System.Reflection;
using ExcelImportExport;
using ExcelImportExport.Attributes;
using ExcelImportExport.Helper;
using ExcelImportExport.Models;
using Microsoft.EntityFrameworkCore;
using SpotAnalysis.Data;
using SpotAnalysis.Data.Enums;
using SpotAnalysis.Data.Models;

namespace SpotAnalysis.Services.Services;

public class XlsImportExportService : IXlsImportExportService
{
    private readonly AnalysisContext _context;

    public XlsImportExportService(AnalysisContext context)
    {
        _context = context;
    }

    // ── Import ──────────────────────────────────────────────────────────

    public async Task ImportFromFileAsync(string filePath)
    {
        using var reader = ExcelImporter.Open(filePath);
        await ImportCoreAsync(reader);
    }

    public async Task ImportFromStreamAsync(Stream stream, ExcelFormat format)
    {
        using var reader = ExcelImporter.Open(stream, format);
        await ImportCoreAsync(reader);
    }

    private async Task ImportCoreAsync(WorkbookReader reader)
    {
        var educts = reader.ReadSheet<Educt>();
        var additives = reader.ReadSheet<Additive>();
        var combinations = reader.ReadSheet<Combination>();

        var methods = await UpsertMethodsAsync();
        var chemicals = await UpsertEductsAsync(educts, methods);
        var additiveChemicals = await UpsertAdditivesAsync(additives);

        foreach (var kvp in additiveChemicals)
            chemicals[kvp.Key] = kvp.Value;

        await UpsertCombinationsAsync(combinations, chemicals);
        await _context.SaveChangesAsync();
    }

    private async Task<Dictionary<string, Method>> UpsertMethodsAsync()
    {
        var methodNames = Educt.MethodNames;
        var existing = await _context.Methods
            .Where(m => methodNames.Contains(m.Name))
            .ToDictionaryAsync(m => m.Name);

        foreach (var name in methodNames)
        {
            if (!existing.ContainsKey(name))
            {
                var method = new Method { Name = name };
                _context.Methods.Add(method);
                existing[name] = method;
            }
        }

        await _context.SaveChangesAsync();
        return existing;
    }

    private async Task<Dictionary<string, Chemical>> UpsertEductsAsync(
        List<Educt> educts, Dictionary<string, Method> methods)
    {
        var names = educts.Select(e => e.Substance).Where(n => n != null).ToList();
        var existing = await _context.Chemicals
            .Include(c => c.MethodOutputs)
            .Where(c => names.Contains(c.Name))
            .ToDictionaryAsync(c => c.Name);

        foreach (var educt in educts)
        {
            if (string.IsNullOrWhiteSpace(educt.Substance)) continue;

            if (!existing.TryGetValue(educt.Substance, out var chemical))
            {
                chemical = new Chemical
                {
                    Name = educt.Substance,
                    Formula = educt.Formula ?? "",
                    Color = educt.InherentColor ?? "keine",
                    Type = ChemicalType.Educt
                };
                _context.Chemicals.Add(chemical);
                existing[educt.Substance] = chemical;
            }
            else
            {
                chemical.Formula = educt.Formula ?? "";
                chemical.Color = educt.InherentColor ?? "keine";
                chemical.Type = ChemicalType.Educt;
            }

            await _context.SaveChangesAsync();

            foreach (var (methodName, value) in educt.GetMethodValues())
            {
                if (methods.TryGetValue(methodName, out var method))
                    UpsertMethodOutput(chemical, method, value);
            }
        }

        await _context.SaveChangesAsync();
        return existing;
    }

    private async Task<Dictionary<string, Chemical>> UpsertAdditivesAsync(List<Additive> additives)
    {
        var names = additives.Select(a => a.Name).Where(n => n != null).ToList();
        var existing = await _context.Chemicals
            .Where(c => names.Contains(c.Name))
            .ToDictionaryAsync(c => c.Name);

        foreach (var additive in additives)
        {
            if (string.IsNullOrWhiteSpace(additive.Name)) continue;

            if (!existing.TryGetValue(additive.Name, out var chemical))
            {
                chemical = new Chemical
                {
                    Name = additive.Name,
                    Formula = additive.Formula ?? "",
                    Color = "keine",
                    Type = ChemicalType.Additive
                };
                _context.Chemicals.Add(chemical);
                existing[additive.Name] = chemical;
            }
            else
            {
                chemical.Formula = additive.Formula ?? "";
                chemical.Type = ChemicalType.Additive;
            }
        }

        await _context.SaveChangesAsync();
        return existing;
    }

    private void UpsertMethodOutput(Chemical chemical, Method method, string? color)
    {
        if (string.IsNullOrWhiteSpace(color)) return;

        var existing = chemical.MethodOutputs
            .FirstOrDefault(mo => mo.MethodID == method.MethodID);

        if (existing != null)
        {
            existing.Color = color;
        }
        else
        {
            var output = new MethodOutput
            {
                ChemicalID = chemical.ChemicalID,
                MethodID = method.MethodID,
                Color = color,
                Chemical = chemical,
                Method = method
            };
            chemical.MethodOutputs.Add(output);
        }
    }

    private static Chemical? FindChemical(Dictionary<string, Chemical> chemicals, string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        // Try by name first, then by formula (Excel "Zusatzstoff" column uses formula)
        if (chemicals.TryGetValue(name, out var chem)) return chem;
        return chemicals.Values.FirstOrDefault(c =>
            string.Equals(c.Formula, name, StringComparison.OrdinalIgnoreCase));
    }

    private async Task UpsertCombinationsAsync(
        List<Combination> combinations, Dictionary<string, Chemical> chemicals)
    {
        var observationTexts = combinations
            .Select(c => c.Observation)
            .Where(o => !string.IsNullOrWhiteSpace(o))
            .Distinct()
            .Cast<string>()
            .ToList();

        var existingObservations = await _context.Observations
            .Where(o => observationTexts.Contains(o.Description))
            .ToDictionaryAsync(o => o.Description);

        foreach (var text in observationTexts)
        {
            if (!existingObservations.ContainsKey(text))
            {
                var obs = new Observation { Description = text };
                _context.Observations.Add(obs);
                existingObservations[text] = obs;
            }
        }

        await _context.SaveChangesAsync();

        var existingReactions = await _context.Reactions
            .ToDictionaryAsync(r => (r.Chemical1ID, r.Chemical2ID));

        foreach (var combo in combinations)
        {
            if (string.IsNullOrWhiteSpace(combo.FirstEductName)) continue;
            if (string.IsNullOrWhiteSpace(combo.Product)) continue;

            var chem1 = FindChemical(chemicals, combo.FirstEductName);
            if (chem1 == null) continue;

            var chem2 = FindChemical(chemicals, combo.SecondEductName)
                     ?? FindChemical(chemicals, combo.AdditiveName);
            if (chem2 == null) continue;

            Observation? observation = null;
            if (!string.IsNullOrWhiteSpace(combo.Observation))
                existingObservations.TryGetValue(combo.Observation, out observation);

            // Normalisierung: Reaction speichert Chemicals immer mit kleinerer ID zuerst (siehe Reaction-Konstruktor + CK_Reaction_ChemicalOrder).
            // Der Lookup-Key muss derselben Normalisierung folgen, sonst wird die Excel-Reihenfolge zur Duplikat-Falle.
            var key = chem1.ChemicalID <= chem2.ChemicalID
                ? (chem1.ChemicalID, chem2.ChemicalID)
                : (chem2.ChemicalID, chem1.ChemicalID);

            if (existingReactions.TryGetValue(key, out var reaction))
            {
                reaction.RelevantProduct = combo.Product;
                reaction.Formula = combo.Formula ?? "";
                if (observation != null)
                    reaction.ObservationID = observation.ObservationID;
            }
            else
            {
                reaction = new Reaction(chem1, chem2)
                {
                    RelevantProduct = combo.Product,
                    Formula = combo.Formula ?? "",
                    ObservationID = observation?.ObservationID ?? 0
                };
                _context.Reactions.Add(reaction);
                existingReactions[key] = reaction;
            }
        }

        await _context.SaveChangesAsync();
    }

    // ── Export ──────────────────────────────────────────────────────────

    public async Task ExportToFileAsync(string filePath)
    {
        var (educts, additives, combinations) = await BuildExportDtosAsync();

        ExcelExporter.ExportMultiSheet(filePath,
            SheetData.From(educts),
            SheetData.From(additives),
            SheetData.From(combinations));
    }

    public async Task ExportToStreamAsync(Stream stream, ExcelFormat format)
    {
        var (educts, additives, combinations) = await BuildExportDtosAsync();

        ExcelExporter.ExportMultiSheet(stream, format,
            SheetData.From(educts),
            SheetData.From(additives),
            SheetData.From(combinations));
    }

    private async Task<(List<Educt>, List<Additive>, List<Combination>)> BuildExportDtosAsync()
    {
        var methods = await _context.Methods.ToDictionaryAsync(m => m.Name);

        var eductChemicals = await _context.Chemicals
            .Include(c => c.MethodOutputs)
            .Where(c => c.Type == ChemicalType.Educt)
            .OrderBy(c => c.ChemicalID)
            .ToListAsync();

        var educts = eductChemicals.Select(c =>
        {
            var educt = new Educt
            {
                Substance = c.Name,
                Formula = c.Formula,
                InherentColor = c.Color
            };
            SetMethodProperties(educt, c, methods);
            return educt;
        }).ToList();

        var additiveChemicals = await _context.Chemicals
            .Where(c => c.Type == ChemicalType.Additive)
            .OrderBy(c => c.ChemicalID)
            .ToListAsync();

        var additives = additiveChemicals.Select(c => new Additive
        {
            Name = c.Name,
            Formula = c.Formula
        }).ToList();

        var allChemicals = await _context.Chemicals.ToDictionaryAsync(c => c.ChemicalID);

        var reactions = await _context.Reactions
            .Include(r => r.Observation)
            .OrderBy(r => r.ReactionID)
            .ToListAsync();

        var combinations = reactions.Select(r =>
        {
            var chem1 = allChemicals[r.Chemical1ID];
            var chem2 = allChemicals[r.Chemical2ID];

            return new Combination
            {
                FirstEductName = chem1.Name,
                SecondEductName = chem2.Type == ChemicalType.Educt ? chem2.Name : null,
                AdditiveName = chem2.Type == ChemicalType.Additive ? chem2.Formula : null,
                Product = r.RelevantProduct,
                Formula = r.Formula,
                Observation = r.Observation?.Description
            };
        }).ToList();

        return (educts, additives, combinations);
    }

    private static void SetMethodProperties(
        Educt educt, Chemical chemical, Dictionary<string, Method> methods)
    {
        foreach (var prop in typeof(Educt).GetProperties())
        {
            var attr = prop.GetCustomAttribute<ExcelColumnAttribute>();
            if (attr is not { IsMethod: true }) continue;

            var methodName = attr.Name ?? prop.Name;
            if (!methods.TryGetValue(methodName, out var method)) continue;

            var color = chemical.MethodOutputs
                .FirstOrDefault(mo => mo.MethodID == method.MethodID)?.Color;
            prop.SetValue(educt, color);
        }
    }
}
