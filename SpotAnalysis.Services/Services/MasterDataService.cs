using Microsoft.EntityFrameworkCore;
using SpotAnalysis.Data;
using SpotAnalysis.Data.Models;
using SpotAnalysis.Services.DTOs;

namespace SpotAnalysis.Services.Services;

public class MasterDataService(IDbContextFactory<AnalysisContext> factory) : IMasterDataService
{
    // ── Chemicals ───────────────────────────────────────────────────

    public async Task<List<ChemicalDetailDto>> GetChemicalsAsync(CancellationToken ct = default)
    {
        await using var context = await factory.CreateDbContextAsync(ct);

        var chemicals = await context.Chemicals
            .Include(c => c.Color)
            .Include(c => c.MethodOutputs)
            .ThenInclude(mo => mo.Method)
            .Include(c => c.MethodOutputs)
            .ThenInclude(mo => mo.Color)
            .Include(c => c.Chemical1Reactions)
            .ThenInclude(r => r.Chemical2)
            .Include(c => c.Chemical2Reactions)
            .ThenInclude(r => r.Chemical1)
            .Include(c => c.StAvailableChemicals)
            .ThenInclude(sac => sac.StQuestion)
            .ThenInclude(stq => stq.Question)
            .Include(c => c.StlQuestions)
            .ThenInclude(stl => stl.Question)
            .AsNoTracking()
            .OrderBy(c => c.Type)
            .ThenBy(c => c.Name)
            .ToListAsync(ct);

        return chemicals.Select(MapChemicalToDetail).ToList();
    }

    public async Task<ChemicalDetailDto?> GetChemicalByIdAsync(int id, CancellationToken ct = default)
    {
        await using var context = await factory.CreateDbContextAsync(ct);

        var chemical = await context.Chemicals
            .Include(c => c.Color)
            .Include(c => c.MethodOutputs)
            .ThenInclude(mo => mo.Method)
            .Include(c => c.MethodOutputs)
            .ThenInclude(mo => mo.Color)
            .Include(c => c.Chemical1Reactions)
            .ThenInclude(r => r.Chemical2)
            .Include(c => c.Chemical2Reactions)
            .ThenInclude(r => r.Chemical1)
            .Include(c => c.StAvailableChemicals)
            .ThenInclude(sac => sac.StQuestion)
            .ThenInclude(stq => stq.Question)
            .Include(c => c.StlQuestions)
            .ThenInclude(stl => stl.Question)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ChemicalId == id, ct);

        return chemical is null ? null : MapChemicalToDetail(chemical);
    }

    public async Task<int> CreateChemicalAsync(ChemicalDetailDto dto, CancellationToken ct = default)
    {
        await using var context = await factory.CreateDbContextAsync(ct);

        var exists = await context.Chemicals.AnyAsync(c =>
            EF.Functions.ILike(c.Name, dto.Name) &&
            EF.Functions.ILike(c.Formula, dto.Formula), ct);
        if (exists)
            throw new InvalidOperationException(
                $"Eine Chemikalie mit Name \"{dto.Name}\" und Formel \"{dto.Formula}\" existiert bereits.");

        var colorId = await ResolveColorIdAsync(context, dto.ColorId, dto.ColorName, ct);

        var chemical = new Chemical
        {
            Name = dto.Name,
            Formula = dto.Formula,
            ColorId = colorId,
            Type = dto.Type,
            ImagePath = dto.ImagePath
        };
        context.Chemicals.Add(chemical);
        await context.SaveChangesAsync(ct);

        await UpsertMethodOutputsAsync(context, chemical, dto.MethodOutputs, ct);
        await context.SaveChangesAsync(ct);

        return chemical.ChemicalId;
    }

    public async Task UpdateChemicalAsync(ChemicalDetailDto dto, CancellationToken ct = default)
    {
        await using var context = await factory.CreateDbContextAsync(ct);

        var chemical = await context.Chemicals.FirstOrDefaultAsync(c => c.ChemicalId == dto.Id, ct)
                       ?? throw new InvalidOperationException($"Chemikalie {dto.Id} nicht gefunden.");

        var duplicate = await context.Chemicals.AnyAsync(c =>
            c.ChemicalId != dto.Id &&
            EF.Functions.ILike(c.Name, dto.Name) &&
            EF.Functions.ILike(c.Formula, dto.Formula), ct);
        if (duplicate)
            throw new InvalidOperationException(
                $"Eine andere Chemikalie mit Name \"{dto.Name}\" und Formel \"{dto.Formula}\" existiert bereits.");

        chemical.Name = dto.Name;
        chemical.Formula = dto.Formula;
        chemical.ColorId = await ResolveColorIdAsync(context, dto.ColorId, dto.ColorName, ct);
        chemical.Type = dto.Type;
        chemical.ImagePath = dto.ImagePath;

        await UpsertMethodOutputsAsync(context, chemical, dto.MethodOutputs, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteChemicalAsync(int id, CancellationToken ct = default)
    {
        await using var context = await factory.CreateDbContextAsync(ct);

        var chemical = await context.Chemicals
                           .Include(c => c.Chemical1Reactions)
                           .ThenInclude(r => r.Chemical2)
                           .Include(c => c.Chemical2Reactions)
                           .ThenInclude(r => r.Chemical1)
                           .Include(c => c.MethodOutputs)
                           .ThenInclude(mo => mo.Method)
                           .Include(c => c.StAvailableChemicals)
                           .ThenInclude(sac => sac.StQuestion)
                           .ThenInclude(stq => stq.Question)
                           .Include(c => c.StlQuestions)
                           .ThenInclude(stl => stl.Question)
                           .FirstOrDefaultAsync(c => c.ChemicalId == id, ct)
                       ?? throw new InvalidOperationException($"Chemikalie {id} nicht gefunden.");

        var references = BuildChemicalReferences(chemical);
        if (references.Total > 0)
            throw new InvalidOperationException(
                $"Chemikalie wird noch referenziert ({references.Total} Einträge) und kann nicht gelöscht werden.");

        context.MethodOutputs.RemoveRange(chemical.MethodOutputs);
        context.Chemicals.Remove(chemical);
        await context.SaveChangesAsync(ct);
    }

    // ── Reactions ───────────────────────────────────────────────────

    public async Task<List<ReactionDetailDto>> GetReactionsAsync(CancellationToken ct = default)
    {
        await using var context = await factory.CreateDbContextAsync(ct);

        var reactions = await context.Reactions
            .Include(r => r.Chemical1)
            .Include(r => r.Chemical2)
            .Include(r => r.Observation)
            .Include(r => r.StlResults)
            .Include(r => r.StlAvailableReactions)
            .ThenInclude(ar => ar.StlQuestion)
            .ThenInclude(stl => stl.Question)
            .Include(r => r.StlQuestions)
            .ThenInclude(stl => stl.Question)
            .AsNoTracking()
            .OrderBy(r => r.Chemical1.Name)
            .ThenBy(r => r.Chemical2.Name)
            .ToListAsync(ct);

        return reactions.Select(MapReactionToDetail).ToList();
    }

    public async Task<ReactionDetailDto?> GetReactionByIdAsync(int id, CancellationToken ct = default)
    {
        await using var context = await factory.CreateDbContextAsync(ct);

        var reaction = await context.Reactions
            .Include(r => r.Chemical1)
            .Include(r => r.Chemical2)
            .Include(r => r.Observation)
            .Include(r => r.StlResults)
            .Include(r => r.StlAvailableReactions)
            .ThenInclude(ar => ar.StlQuestion)
            .ThenInclude(stl => stl.Question)
            .Include(r => r.StlQuestions)
            .ThenInclude(stl => stl.Question)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ReactionId == id, ct);

        return reaction is null ? null : MapReactionToDetail(reaction);
    }

    public async Task<int> CreateReactionAsync(ReactionDetailDto dto, CancellationToken ct = default)
    {
        await using var context = await factory.CreateDbContextAsync(ct);

        if (dto.Chemical1Id == dto.Chemical2Id)
            throw new InvalidOperationException("Chemikalie A und Chemikalie B müssen unterschiedlich sein.");

        var chem1 = await context.Chemicals.FirstOrDefaultAsync(c => c.ChemicalId == dto.Chemical1Id, ct)
                    ?? throw new InvalidOperationException($"Chemikalie {dto.Chemical1Id} nicht gefunden.");
        var chem2 = await context.Chemicals.FirstOrDefaultAsync(c => c.ChemicalId == dto.Chemical2Id, ct)
                    ?? throw new InvalidOperationException($"Chemikalie {dto.Chemical2Id} nicht gefunden.");

        var (lowId, highId) = chem1.ChemicalId <= chem2.ChemicalId
            ? (chem1.ChemicalId, chem2.ChemicalId)
            : (chem2.ChemicalId, chem1.ChemicalId);

        var duplicate = await context.Reactions.AnyAsync(r =>
            r.Chemical1Id == lowId && r.Chemical2Id == highId, ct);
        if (duplicate)
            throw new InvalidOperationException(
                "Für diese Chemikalien-Kombination existiert bereits eine Reaktion.");

        var observation = await ResolveObservationAsync(context, dto, ct);

        var reaction = new Reaction(chem1, chem2)
        {
            RelevantProduct = dto.RelevantProduct,
            Formula = dto.Formula,
            ObservationId = observation.ObservationId,
            ImagePath = dto.ImagePath
        };
        context.Reactions.Add(reaction);
        await context.SaveChangesAsync(ct);

        return reaction.ReactionId;
    }

    public async Task UpdateReactionAsync(ReactionDetailDto dto, CancellationToken ct = default)
    {
        await using var context = await factory.CreateDbContextAsync(ct);

        var reaction = await context.Reactions.FirstOrDefaultAsync(r => r.ReactionId == dto.Id, ct)
                       ?? throw new InvalidOperationException($"Reaktion {dto.Id} nicht gefunden.");

        if (dto.Chemical1Id == dto.Chemical2Id)
            throw new InvalidOperationException("Chemikalie A und Chemikalie B müssen unterschiedlich sein.");

        var chem1 = await context.Chemicals.FirstOrDefaultAsync(c => c.ChemicalId == dto.Chemical1Id, ct)
                    ?? throw new InvalidOperationException($"Chemikalie {dto.Chemical1Id} nicht gefunden.");
        var chem2 = await context.Chemicals.FirstOrDefaultAsync(c => c.ChemicalId == dto.Chemical2Id, ct)
                    ?? throw new InvalidOperationException($"Chemikalie {dto.Chemical2Id} nicht gefunden.");

        var (lowId, highId) = chem1.ChemicalId <= chem2.ChemicalId
            ? (chem1.ChemicalId, chem2.ChemicalId)
            : (chem2.ChemicalId, chem1.ChemicalId);

        var duplicate = await context.Reactions.AnyAsync(r =>
            r.ReactionId != dto.Id &&
            r.Chemical1Id == lowId && r.Chemical2Id == highId, ct);
        if (duplicate)
            throw new InvalidOperationException(
                "Für diese Chemikalien-Kombination existiert bereits eine andere Reaktion.");

        var observation = await ResolveObservationAsync(context, dto, ct);

        reaction.SetChemicals(chem1, chem2);
        reaction.RelevantProduct = dto.RelevantProduct;
        reaction.Formula = dto.Formula;
        reaction.ObservationId = observation.ObservationId;
        reaction.ImagePath = dto.ImagePath;

        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteReactionAsync(int id, CancellationToken ct = default)
    {
        await using var context = await factory.CreateDbContextAsync(ct);

        var reaction = await context.Reactions
                           .Include(r => r.StlResults)
                           .Include(r => r.StlAvailableReactions)
                           .ThenInclude(ar => ar.StlQuestion)
                           .ThenInclude(stl => stl.Question)
                           .Include(r => r.StlQuestions)
                           .ThenInclude(stl => stl.Question)
                           .FirstOrDefaultAsync(r => r.ReactionId == id, ct)
                       ?? throw new InvalidOperationException($"Reaktion {id} nicht gefunden.");

        var references = BuildReactionReferences(reaction);
        if (references.Total > 0)
            throw new InvalidOperationException(
                $"Reaktion wird noch referenziert ({references.Total} Einträge) und kann nicht gelöscht werden.");

        context.Reactions.Remove(reaction);
        await context.SaveChangesAsync(ct);
    }

    // ── Observations ────────────────────────────────────────────────

    public async Task<List<ObservationDetailDto>> GetObservationsAsync(CancellationToken ct = default)
    {
        await using var context = await factory.CreateDbContextAsync(ct);

        var observations = await context.Observations
            .Include(o => o.Reactions)
            .ThenInclude(r => r.Chemical1)
            .Include(o => o.Reactions)
            .ThenInclude(r => r.Chemical2)
            .AsNoTracking()
            .OrderBy(o => o.Description)
            .ToListAsync(ct);

        return observations.Select(o => new ObservationDetailDto
        {
            Id = o.ObservationId,
            Description = o.Description,
            References = new ReferenceReport
            {
                Items = o.Reactions.Select(r => new ReferenceItem
                {
                    Kind = "Reaction",
                    Id = r.ReactionId,
                    Description = $"Reaktion #{r.ReactionId}: {r.Chemical1.Name} + {r.Chemical2.Name}"
                }).ToList()
            }
        }).ToList();
    }

    public async Task<int> CreateObservationAsync(ObservationDetailDto dto, CancellationToken ct = default)
    {
        await using var context = await factory.CreateDbContextAsync(ct);
        var desc = dto.Description.Trim();

        var duplicate = await context.Observations.AnyAsync(o =>
            EF.Functions.ILike(o.Description, desc), ct);
        if (duplicate)
            throw new InvalidOperationException(
                $"Eine Beobachtung mit Beschreibung \"{desc}\" existiert bereits.");

        var obs = new Observation { Description = desc };
        context.Observations.Add(obs);
        await context.SaveChangesAsync(ct);
        return obs.ObservationId;
    }

    public async Task UpdateObservationAsync(ObservationDetailDto dto, CancellationToken ct = default)
    {
        await using var context = await factory.CreateDbContextAsync(ct);
        var desc = dto.Description.Trim();

        var obs = await context.Observations.FirstOrDefaultAsync(o => o.ObservationId == dto.Id, ct)
                  ?? throw new InvalidOperationException($"Beobachtung {dto.Id} nicht gefunden.");

        var duplicate = await context.Observations.AnyAsync(o =>
            o.ObservationId != dto.Id && EF.Functions.ILike(o.Description, desc), ct);
        if (duplicate)
            throw new InvalidOperationException(
                $"Eine andere Beobachtung mit Beschreibung \"{desc}\" existiert bereits.");

        obs.Description = desc;
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteObservationAsync(int id, CancellationToken ct = default)
    {
        await using var context = await factory.CreateDbContextAsync(ct);

        var obs = await context.Observations
                      .Include(o => o.Reactions)
                      .FirstOrDefaultAsync(o => o.ObservationId == id, ct)
                  ?? throw new InvalidOperationException($"Beobachtung {id} nicht gefunden.");

        if (obs.Reactions.Count > 0)
            throw new InvalidOperationException(
                $"Beobachtung wird von {obs.Reactions.Count} Reaktionen verwendet und kann nicht gelöscht werden.");

        context.Observations.Remove(obs);
        await context.SaveChangesAsync(ct);
    }

    // ── Helpers: Chemicals ──────────────────────────────────────────

    private static ChemicalDetailDto MapChemicalToDetail(Chemical c) => new()
    {
        Id = c.ChemicalId,
        Name = c.Name,
        Formula = c.Formula,
        ColorId = c.ColorId,
        ColorName = c.Color.Name,
        Type = c.Type,
        ImagePath = c.ImagePath,
        MethodOutputs = c.MethodOutputs
            .OrderBy(mo => mo.Method.Name)
            .Select(mo => new MethodOutputEntry
            {
                MethodId = mo.MethodId, MethodName = mo.Method.Name, ColorId = mo.ColorId, ColorName = mo.Color.Name
            }).ToList(),
        References = BuildChemicalReferences(c)
    };

    private static ReferenceReport BuildChemicalReferences(Chemical c)
    {
        var report = new ReferenceReport();
        foreach (var r in c.Chemical1Reactions.Concat(c.Chemical2Reactions))
            report.Items.Add(new ReferenceItem
            {
                Kind = "Reaction",
                Id = r.ReactionId,
                Description = $"Reaktion #{r.ReactionId}: {r.Chemical1.Name} + {r.Chemical2.Name}"
            });
        foreach (var sac in c.StAvailableChemicals)
        {
            var title = sac.StQuestion?.Question?.Title;
            report.Items.Add(new ReferenceItem
            {
                Kind = "Quiz",
                Id = sac.QuestionId,
                Description = string.IsNullOrWhiteSpace(title) ? $"Frage #{sac.QuestionId}" : title,
                RouteTemplate = "/teacher/questions/{0}/edit?type=SpotTest"
            });
        }

        foreach (var stl in c.StlQuestions)
        {
            var title = stl.Question?.Title;
            report.Items.Add(new ReferenceItem
            {
                Kind = "Quiz",
                Id = stl.QuestionId,
                Description = string.IsNullOrWhiteSpace(title) ? $"Frage #{stl.QuestionId}" : title,
                RouteTemplate = "/teacher/questions/{0}/edit?type=SpotTestLight"
            });
        }

        return report;
    }

    // ── Helpers: Reactions ──────────────────────────────────────────

    private static ReactionDetailDto MapReactionToDetail(Reaction r) => new()
    {
        Id = r.ReactionId,
        Chemical1Id = r.Chemical1.ChemicalId,
        Chemical2Id = r.Chemical2.ChemicalId,
        Chemical1Name = r.Chemical1.Name,
        Chemical2Name = r.Chemical2.Name,
        RelevantProduct = r.RelevantProduct,
        Formula = r.Formula,
        ObservationId = r.ObservationId == 0 ? null : r.ObservationId,
        ObservationDescription = r.Observation?.Description ?? "",
        ImagePath = r.ImagePath,
        References = BuildReactionReferences(r)
    };

    private static ReferenceReport BuildReactionReferences(Reaction r)
    {
        var report = new ReferenceReport();
        foreach (var q in r.StlQuestions)
        {
            var title = q.Question?.Title;
            report.Items.Add(new ReferenceItem
            {
                Kind = "Quiz",
                Id = q.QuestionId,
                Description = string.IsNullOrWhiteSpace(title) ? $"Frage #{q.QuestionId}" : title,
                RouteTemplate = "/teacher/questions/{0}/edit?type=SpotTestLight"
            });
        }

        foreach (var ar in r.StlAvailableReactions)
        {
            var title = ar.StlQuestion?.Question?.Title;
            report.Items.Add(new ReferenceItem
            {
                Kind = "Quiz",
                Id = ar.QuestionId,
                Description = string.IsNullOrWhiteSpace(title) ? $"Frage #{ar.QuestionId}" : title
            });
        }

        return report;
    }

    private static async Task<Observation> ResolveObservationAsync(
        AnalysisContext context, ReactionDetailDto dto, CancellationToken ct)
    {
        if (dto.ObservationId is { } id)
        {
            return await context.Observations.FirstOrDefaultAsync(o => o.ObservationId == id, ct)
                   ?? throw new InvalidOperationException($"Beobachtung {id} nicht gefunden.");
        }

        var desc = (dto.NewObservationDescription ?? "").Trim();
        if (string.IsNullOrEmpty(desc))
            throw new InvalidOperationException("Beobachtung muss ausgewählt oder eingegeben werden.");

        var existing = await context.Observations
            .FirstOrDefaultAsync(o => EF.Functions.ILike(o.Description, desc), ct);
        if (existing is not null)
            return existing;

        var created = new Observation { Description = desc };
        context.Observations.Add(created);
        await context.SaveChangesAsync(ct);
        return created;
    }

    // ── Helpers: Color resolution ─────────────────────────────────────

    private static async Task<int> ResolveColorIdAsync(
        AnalysisContext context, int colorId, string colorName, CancellationToken ct)
    {
        if (colorId > 0)
            return colorId;

        var color = await context.Colors.FirstOrDefaultAsync(
            c => EF.Functions.ILike(c.Name, colorName), ct);
        if (color is not null)
            return color.ColorId;

        color = new Color { Name = colorName, HexValue = "#666666" };
        context.Colors.Add(color);
        await context.SaveChangesAsync(ct);
        return color.ColorId;
    }

    // ── Helpers: MethodOutputs ──────────────────────────────────────

    private static async Task UpsertMethodOutputsAsync(
        AnalysisContext context, Chemical chemical, List<MethodOutputEntry> entries, CancellationToken ct)
    {
        var methodsByName = await context.Methods.ToDictionaryAsync(m => m.Name, ct);

        var existingOutputs = await context.MethodOutputs
            .Where(mo => mo.ChemicalId == chemical.ChemicalId)
            .ToListAsync(ct);

        foreach (var entry in entries)
        {
            if (!methodsByName.TryGetValue(entry.MethodName, out var method))
                continue; // unbekannte Method überspringen

            var existing = existingOutputs.FirstOrDefault(mo => mo.MethodId == method.MethodId);

            if (entry.ColorId == 0 && string.IsNullOrWhiteSpace(entry.ColorName))
            {
                if (existing is not null)
                    context.MethodOutputs.Remove(existing);
            }
            else
            {
                var colorId = await ResolveColorIdAsync(context, entry.ColorId, entry.ColorName, ct);
                if (existing is null)
                {
                    context.MethodOutputs.Add(new MethodOutput
                    {
                        ChemicalId = chemical.ChemicalId, MethodId = method.MethodId, ColorId = colorId
                    });
                }
                else
                {
                    existing.ColorId = colorId;
                }
            }
        }
    }
}
