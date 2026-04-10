using Microsoft.EntityFrameworkCore;
using SpotAnalysis.Data;
using SpotAnalysis.Services.Services;

namespace SpotAnalysis.Services.Tests;

/// <summary>
/// Manuelle Tests gegen die echte SpotAnalysis-Datenbank.
/// Werden NUR ausgeführt, wenn sie explizit im Test-Runner ausgewählt werden.
/// </summary>
[TestFixture, Explicit("Läuft gegen die echte SpotAnalysis-DB — nur manuell ausführen")]
public class ManualImportExport
{
    private static readonly string TestSheetDir = Path.Combine(
        TestContext.CurrentContext.TestDirectory, "TestSheet");

    private static readonly string ImportFile = Path.Combine(TestSheetDir, "Tuepfel_Import_Export.xlsx");

    private AnalysisContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AnalysisContext>()
            .UseSqlServer(TestConfiguration.GetConnectionString("MyDatabase"))
            .Options;
        return new AnalysisContext(options);
    }

    [Test]
    public async Task Import_IntoSpotAnalysisDb()
    {
        await using var context = CreateContext();
        var service = new XlsImportExportService(context);

        await service.ImportFromFileAsync(ImportFile);

        var chemicals = await context.Chemicals.CountAsync();
        var reactions = await context.Reactions.CountAsync();
        var methods = await context.Methods.CountAsync();

        TestContext.WriteLine($"Import abgeschlossen: {chemicals} Chemicals, {reactions} Reactions, {methods} Methods");
    }

    [Test]
    public async Task Delete_ImportedData()
    {
        await using var context = CreateContext();

        // Reihenfolge beachten wegen Foreign Keys:
        // Reactions → MethodOutputs → Observations → Chemicals → Methods

        var reactions = await context.Reactions.CountAsync();
        context.Reactions.RemoveRange(context.Reactions);
        await context.SaveChangesAsync();

        var methodOutputs = await context.MethodOutputs.CountAsync();
        context.MethodOutputs.RemoveRange(context.MethodOutputs);
        await context.SaveChangesAsync();

        var observations = await context.Observations.CountAsync();
        context.Observations.RemoveRange(context.Observations);
        await context.SaveChangesAsync();

        var chemicals = await context.Chemicals.CountAsync();
        context.Chemicals.RemoveRange(context.Chemicals);
        await context.SaveChangesAsync();

        var methods = await context.Methods.CountAsync();
        context.Methods.RemoveRange(context.Methods);
        await context.SaveChangesAsync();

        TestContext.WriteLine($"Gelöscht: {reactions} Reactions, {methodOutputs} MethodOutputs, {observations} Observations, {chemicals} Chemicals, {methods} Methods");
    }

    [Test]
    public async Task Export_FromSpotAnalysisDb()
    {
        var exportPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "SpotAnalysis_Export.xlsx");

        await using var context = CreateContext();
        var service = new XlsImportExportService(context);

        await service.ExportToFileAsync(exportPath);

        TestContext.WriteLine($"Export gespeichert: {exportPath}");
        Assert.That(File.Exists(exportPath), Is.True);
    }
}
