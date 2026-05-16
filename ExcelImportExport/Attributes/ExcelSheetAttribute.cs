namespace ExcelImportExport.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ExcelSheetAttribute : Attribute
{
    public ExcelSheetAttribute(string? name = null)
    {
        Name = name;
    }

    public string? Name { get; set; }
}
