using ExcelImportExport.Attributes;

namespace ExcelImportExport.Tests.Models;

[ExcelSheet("Kombinationen")]
public class Combination
{
    [ExcelColumn("Edukt 1", Order = 1)]
    public string? FirstEductName { get; set; }

    [ExcelColumn("Edukt 2", Order = 2)]
    public string? SecondEductName { get; set; }

    [ExcelColumn("Zusatzstoff", Order = 3)]
    public string? AdditiveName { get; set; }

    [ExcelColumn("Produkt", Order = 4)]
    public string? Product { get; set; }

    [ExcelColumn("Formel", Order = 5)]
    public string? Formula { get; set; }

    [ExcelColumn("Beobachtung", Order = 6)]
    public string? Observation { get; set; }
}
