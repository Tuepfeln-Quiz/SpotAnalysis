using ExcelImportExport.Helper;
using ExcelImportExport.Models;
using NUnit.Framework;

namespace ExcelImportExport.Tests;

[TestFixture]
public class ExcelRoundtripTests
{
    private static readonly string TestSheetDir = Path.Combine(
        TestContext.CurrentContext.TestDirectory, "TestSheet");

    private static readonly string ImportFile = Path.Combine(TestSheetDir, "Tuepfel_Import_Export.xlsx");
    private static readonly string ExportFile = Path.Combine(TestSheetDir, "Tuepfel_Export_Test.xlsx");

    private List<Educt> _educts = null!;
    private List<Additive> _additives = null!;
    private List<Combination> _combinations = null!;

    [OneTimeSetUp]
    public void ImportFromSource()
    {
        Assert.That(File.Exists(ImportFile), Is.True, $"Testdatei nicht gefunden: {ImportFile}");

        using var reader = ExcelImporter.Open(ImportFile);
        _educts = reader.ReadSheet<Educt>();
        _additives = reader.ReadSheet<Additive>();
        _combinations = reader.ReadSheet<Combination>();
    }

    [OneTimeTearDown]
    public void Cleanup()
    {
        if (File.Exists(ExportFile))
            File.Delete(ExportFile);
    }

    // --- Import Asserts ---

    [Test, Order(1)]
    public void Import_Educts_HasExpectedCount()
    {
        Assert.That(_educts, Has.Count.EqualTo(7));
    }

    [Test, Order(1)]
    public void Import_Educts_FirstEntryIsCorrect()
    {
        var first = _educts[0];
        Assert.That(first.Substance, Is.EqualTo("Eisen(III)chlorid"));
        Assert.That(first.Formula, Is.EqualTo("FeCl3"));
        Assert.That(first.InherentColor, Is.EqualTo("orange"));
        Assert.That(first.PhPaper, Is.EqualTo("rot"));
        Assert.That(first.FlameColor, Is.EqualTo("keine"));
    }

    [Test, Order(1)]
    public void Import_Additives_HasExpectedCount()
    {
        Assert.That(_additives, Has.Count.EqualTo(2));
    }

    [Test, Order(1)]
    public void Import_Additives_DataIsCorrect()
    {
        Assert.That(_additives[0].Name, Is.EqualTo("Natriumhydroxid"));
        Assert.That(_additives[0].Formula, Is.EqualTo("NaOH"));
        Assert.That(_additives[1].Name, Is.EqualTo("Salzsäure"));
        Assert.That(_additives[1].Formula, Is.EqualTo("HCl"));
    }

    [Test, Order(1)]
    public void Import_Combinations_HasExpectedCount()
    {
        Assert.That(_combinations, Has.Count.EqualTo(35));
    }

    [Test, Order(1)]
    public void Import_Combinations_FirstEntryIsCorrect()
    {
        var first = _combinations[0];
        Assert.That(first.FirstEductName, Is.EqualTo("Eisen(III)chlorid"));
        Assert.That(first.SecondEductName, Is.EqualTo("Blei(II)nitrat"));
        Assert.That(first.Product, Is.EqualTo("Blei(II)chlorid"));
        Assert.That(first.Formula, Is.EqualTo("PbCl2"));
        Assert.That(first.Observation, Is.EqualTo("weißer Niederschlag"));
    }

    // --- Export + Re-Import Roundtrip ---

    [Test, Order(2)]
    public void Export_CreatesFile()
    {
        ExcelExporter.ExportMultiSheet(ExportFile,
            SheetData.From(_educts),
            SheetData.From(_additives),
            SheetData.From(_combinations));

        Assert.That(File.Exists(ExportFile), Is.True);
    }

    [Test, Order(3)]
    public void Roundtrip_Educts_MatchAfterExportAndReImport()
    {
        using var reader = ExcelImporter.Open(ExportFile);
        var reimported = reader.ReadSheet<Educt>();

        Assert.That(reimported, Has.Count.EqualTo(_educts.Count));
        for (var i = 0; i < _educts.Count; i++)
        {
            Assert.That(reimported[i].Substance, Is.EqualTo(_educts[i].Substance), $"Educt[{i}].Substance");
            Assert.That(reimported[i].Formula, Is.EqualTo(_educts[i].Formula), $"Educt[{i}].Formula");
            Assert.That(reimported[i].InherentColor, Is.EqualTo(_educts[i].InherentColor), $"Educt[{i}].InherentColor");
            Assert.That(reimported[i].PhPaper, Is.EqualTo(_educts[i].PhPaper), $"Educt[{i}].PhPaper");
            Assert.That(reimported[i].FlameColor, Is.EqualTo(_educts[i].FlameColor), $"Educt[{i}].FlameColor");
        }
    }

    [Test, Order(3)]
    public void Roundtrip_Additives_MatchAfterExportAndReImport()
    {
        using var reader = ExcelImporter.Open(ExportFile);
        var reimported = reader.ReadSheet<Additive>();

        Assert.That(reimported, Has.Count.EqualTo(_additives.Count));
        for (var i = 0; i < _additives.Count; i++)
        {
            Assert.That(reimported[i].Name, Is.EqualTo(_additives[i].Name), $"Additive[{i}].Name");
            Assert.That(reimported[i].Formula, Is.EqualTo(_additives[i].Formula), $"Additive[{i}].Formula");
        }
    }

    [Test, Order(3)]
    public void Roundtrip_Combinations_MatchAfterExportAndReImport()
    {
        using var reader = ExcelImporter.Open(ExportFile);
        var reimported = reader.ReadSheet<Combination>();

        Assert.That(reimported, Has.Count.EqualTo(_combinations.Count));
        for (var i = 0; i < _combinations.Count; i++)
        {
            Assert.That(reimported[i].FirstEductName, Is.EqualTo(_combinations[i].FirstEductName), $"Combination[{i}].FirstEductName");
            Assert.That(reimported[i].SecondEductName, Is.EqualTo(_combinations[i].SecondEductName), $"Combination[{i}].SecondEductName");
            Assert.That(reimported[i].AdditiveName, Is.EqualTo(_combinations[i].AdditiveName), $"Combination[{i}].AdditiveName");
            Assert.That(reimported[i].Product, Is.EqualTo(_combinations[i].Product), $"Combination[{i}].Product");
            Assert.That(reimported[i].Formula, Is.EqualTo(_combinations[i].Formula), $"Combination[{i}].Formula");
            Assert.That(reimported[i].Observation, Is.EqualTo(_combinations[i].Observation), $"Combination[{i}].Observation");
        }
    }
}
