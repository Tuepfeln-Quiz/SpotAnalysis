using ExcelImportExport;
using ExcelImportExport.Helper;
using ExcelImportExport.Models;
using Microsoft.EntityFrameworkCore;
using SpotAnalysis.Data;
using SpotAnalysis.Data.Enums;
using SpotAnalysis.Data.Models;
using SpotAnalysis.Services.Services;

namespace SpotAnalysis.Services.Tests;

[TestFixture]
public class TestXlsImportExportService : BaseDatabaseTest
{
    private static readonly string TestSheetDir = Path.Combine(
        TestContext.CurrentContext.TestDirectory, "TestSheet");

    private static readonly string ImportFile = Path.Combine(TestSheetDir, "Tuepfel_Import_Export.xlsx");
    private static readonly string ExportFile = Path.Combine(TestSheetDir, "Tuepfel_Service_Export.xlsx");

    // ── Import ──────────────────────────────────────────────────────

    [Test, Order(1)]
    public async Task Import_CreatesChemicals()
    {
        var context = ContextFactory.CreateDbContext();
        var service = new XlsImportExportService(context);

        using var reader = ExcelImporter.Open(ImportFile);
        var expectedEductNames = reader.ReadSheet<Educt>()
            .Select(e => e.Substance)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .ToList();
        var expectedAdditiveNames = reader.ReadSheet<Additive>()
            .Select(a => a.Name)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .ToList();

        await service.ImportFromFileAsync(ImportFile);

        var chemicals = await context.Chemicals.ToListAsync();
        var importedEductNames = chemicals.Where(c => c.Type == ChemicalType.Educt).Select(c => c.Name);
        var importedAdditiveNames = chemicals.Where(c => c.Type == ChemicalType.Additive).Select(c => c.Name);

        Assert.That(importedEductNames, Is.SupersetOf(expectedEductNames),
            "Alle Excel-Edukte muessen als Educt-Chemicals importiert sein");
        Assert.That(importedAdditiveNames, Is.SupersetOf(expectedAdditiveNames),
            "Alle Excel-Additive muessen als Additive-Chemicals importiert sein");
    }

    [Test, Order(2)]
    public async Task Import_CreatesMethods()
    {
        var context = ContextFactory.CreateDbContext();

        var methods = await context.Methods.ToListAsync();
        Assert.That(methods.Select(m => m.Name),
            Is.SupersetOf(Educt.MethodNames));
    }

    [Test, Order(2)]
    public async Task Import_CreatesMethodOutputs()
    {
        var context = ContextFactory.CreateDbContext();

        var feCl3 = await context.Chemicals
            .Include(c => c.MethodOutputs)
            .ThenInclude(mo => mo.Method)
            .FirstAsync(c => c.Name == "Eisen(III)chlorid");

        Assert.That(feCl3.Color, Is.EqualTo("orange"));

        Assert.That(feCl3.MethodOutputs.Any(mo => mo.Method.Name == "Eigenfarbe"), Is.False,
            "Eigenfarbe darf nicht mehr als MethodOutput importiert werden");

        var phPapier = feCl3.MethodOutputs.First(mo => mo.Method.Name == "ph-Papier");
        Assert.That(phPapier.Color, Is.EqualTo("rot"));

        var flamme = feCl3.MethodOutputs.First(mo => mo.Method.Name == "Flammenfärbung");
        Assert.That(flamme.Color, Is.EqualTo("keine"));
    }

    [Test, Order(2)]
    public async Task Import_CreatesReactionsAndObservations()
    {
        var context = ContextFactory.CreateDbContext();

        var reactions = await context.Reactions
            .Include(r => r.Observation)
            .ToListAsync();
        Assert.That(reactions, Has.Count.GreaterThan(0), "Es sollten Reactions importiert worden sein");

        var observations = await context.Observations.ToListAsync();
        Assert.That(observations, Has.Count.GreaterThan(0), "Es sollten Observations importiert worden sein");

        // Prüfe, dass jede Reaction eine Observation hat
        Assert.That(reactions, Has.All.Matches<Reaction>(r => r.Observation != null));
    }

    [Test, Order(3)]
    public async Task Import_Upsert_DoesNotDuplicate()
    {
        var context = ContextFactory.CreateDbContext();
        var service = new XlsImportExportService(context);

        var countBefore = await context.Chemicals.CountAsync();
        await service.ImportFromFileAsync(ImportFile);
        var countAfter = await context.Chemicals.CountAsync();

        Assert.That(countAfter, Is.EqualTo(countBefore));
    }

    // ── Export ──────────────────────────────────────────────────────

    [Test, Order(4)]
    public async Task Export_CreatesFileWithAllSheets()
    {
        var context = ContextFactory.CreateDbContext();
        var service = new XlsImportExportService(context);

        await service.ExportToFileAsync(ExportFile);

        Assert.That(File.Exists(ExportFile), Is.True);

        using var reader = ExcelImporter.Open(ExportFile);
        Assert.That(reader.SheetNames, Is.SupersetOf(new[] { "Edukte", "Zusatzstoffe", "Kombinationen" }));
    }

    [Test, Order(5)]
    public async Task Export_EductsMatchDatabase()
    {
        var context = ContextFactory.CreateDbContext();
        var dbEductNames = await context.Chemicals
            .Where(c => c.Type == ChemicalType.Educt)
            .Select(c => c.Name)
            .ToListAsync();

        using var reader = ExcelImporter.Open(ExportFile);
        var educts = reader.ReadSheet<Educt>();

        Assert.That(educts.Select(e => e.Substance), Is.EquivalentTo(dbEductNames),
            "Export muss exakt die Educt-Chemicals der DB enthalten");

        var feCl3 = educts.First(e => e.Substance == "Eisen(III)chlorid");
        Assert.That(feCl3.InherentColor, Is.EqualTo("orange"));
        Assert.That(feCl3.PhPaper, Is.EqualTo("rot"));
        Assert.That(feCl3.FlameColor, Is.EqualTo("keine"));
    }

    [Test, Order(5)]
    public async Task Export_AdditivesMatchDatabase()
    {
        using var reader = ExcelImporter.Open(ExportFile);
        var additives = reader.ReadSheet<Additive>();

        Assert.That(additives, Has.Count.EqualTo(2));
        Assert.That(additives.Select(a => a.Name),
            Is.SupersetOf(new[] { "Natriumhydroxid", "Salzsäure" }));
    }

    [Test, Order(5)]
    public async Task Export_CombinationsMatchDatabase()
    {
        var context = ContextFactory.CreateDbContext();
        var expectedCount = await context.Reactions.CountAsync();

        using var reader = ExcelImporter.Open(ExportFile);
        var combinations = reader.ReadSheet<Combination>();

        Assert.That(combinations, Has.Count.EqualTo(expectedCount));
        Assert.That(combinations, Has.All.Matches<Combination>(c =>
            !string.IsNullOrWhiteSpace(c.FirstEductName)));

        // Jede Kombination hat entweder SecondEductName oder AdditiveName, nie beides
        Assert.That(combinations, Has.All.Matches<Combination>(c =>
            !string.IsNullOrWhiteSpace(c.SecondEductName) || !string.IsNullOrWhiteSpace(c.AdditiveName)));

        // Produkt und Formel sind immer gesetzt
        Assert.That(combinations, Has.All.Matches<Combination>(c =>
            !string.IsNullOrWhiteSpace(c.Product) && !string.IsNullOrWhiteSpace(c.Formula)));
    }

    [OneTimeTearDown]
    public new void GlobalTeardown()
    {
        if (File.Exists(ExportFile))
            File.Delete(ExportFile);
    }
}
