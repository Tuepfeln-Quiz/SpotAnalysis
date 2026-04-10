using ExcelImportExport.Attributes;

namespace ExcelImportExport.Models;

[ExcelSheet("Zusatzstoffe")]
public class Additive
{
    [ExcelColumn("Zusatzstoff", Order = 1)]
    public string? Name { get; set; }

    [ExcelColumn("Formel", Order = 2)]
    public string? Formula { get; set; }
}
