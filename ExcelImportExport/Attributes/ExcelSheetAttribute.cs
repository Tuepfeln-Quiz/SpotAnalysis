namespace ExcelImportExport.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ExcelSheetAttribute : Attribute
{
    public string? Name { get; set; }

    public ExcelSheetAttribute(string? name = null)
    {
        Name = name;
    }
}