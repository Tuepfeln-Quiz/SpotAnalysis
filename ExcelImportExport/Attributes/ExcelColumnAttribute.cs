namespace ExcelImportExport.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ExcelColumnAttribute : Attribute
{
    public string? Name { get; set; }
    public int Order { get; set; } = int.MaxValue;
    public bool Ignore { get; set; }
    public bool IsMethod { get; set; }

    public ExcelColumnAttribute(string? name = null)
    {
        Name = name;
    }
}
