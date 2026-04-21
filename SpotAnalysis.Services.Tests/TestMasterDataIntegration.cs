using ExcelImportExport.Helper;
using Microsoft.EntityFrameworkCore;
using SpotAnalysis.Services.Services;

namespace SpotAnalysis.Services.Tests;

[TestFixture]
public class TestMasterDataIntegration : BaseDatabaseTest
{
    [SetUp]
    public async Task Reset() => await CleanUpDb();

    [Test]
    public async Task RoundTrip_Export_Import_KeepsChemicalCount()
    {
        var context = await ContextFactory.CreateDbContextAsync();
        var xls = new XlsImportExportService(context);
        await using var export = new MemoryStream();
        await xls.ExportToStreamAsync(export, ExcelFormat.Xlsx);
        var countBefore = await context.Chemicals.CountAsync();

        // Clean and re-import
        await CleanUpDb();
        context = await ContextFactory.CreateDbContextAsync();
        xls = new XlsImportExportService(context);
        export.Position = 0;
        await xls.ImportFromStreamAsync(export, ExcelFormat.Xlsx);

        var countAfter = await context.Chemicals.CountAsync();
        Assert.That(countAfter, Is.GreaterThanOrEqualTo(countBefore));
    }
}
