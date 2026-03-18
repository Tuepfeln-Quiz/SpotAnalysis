using ExcelImportExport.Attributes;

namespace ExcelImportExport.Tests.Models;

[ExcelSheet("Edukte")]
public class Educt
{
    [ExcelColumn("Ausgangsstoff", Order = 1)]
    public string? Substance { get; set; }

    [ExcelColumn("Formel", Order = 2)]
    public string? Formula { get; set; }

    [ExcelColumn("Eigenfarbe", Order = 3)]
    public string? InherentColor { get; set; }

    [ExcelColumn("ph-Papier", Order = 4)]
    public string? PhPaper { get; set; }

    [ExcelColumn("Flammenfärbung", Order = 5)]
    public string? FlameColor { get; set; }
}
