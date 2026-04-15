using System.Reflection;
using ExcelImportExport.Attributes;

namespace ExcelImportExport.Models;

[ExcelSheet("Edukte")]
public class Educt
{
    [ExcelColumn("Ausgangsstoff", Order = 1)]
    public string? Substance { get; set; }

    [ExcelColumn("Formel", Order = 2)]
    public string? Formula { get; set; }

    [ExcelColumn("Eigenfarbe", Order = 3)]
    public string? InherentColor { get; set; }

    [ExcelColumn("ph-Papier", Order = 4, IsMethod = true)]
    public string? PhPaper { get; set; }

    [ExcelColumn("Flammenfärbung", Order = 5, IsMethod = true)]
    public string? FlameColor { get; set; }

    /// <summary>
    /// Returns all properties marked with IsMethod=true as (MethodName, Value) pairs.
    /// </summary>
    public IEnumerable<(string MethodName, string? Value)> GetMethodValues()
    {
        foreach (var prop in GetType().GetProperties())
        {
            var attr = prop.GetCustomAttribute<ExcelColumnAttribute>();
            if (attr is not { IsMethod: true }) continue;
            yield return (attr.Name ?? prop.Name, (string?)prop.GetValue(this));
        }
    }

    /// <summary>
    /// Returns the column names of all method properties.
    /// </summary>
    public static IReadOnlyList<string> MethodNames =>
        typeof(Educt).GetProperties()
            .Select(p => p.GetCustomAttribute<ExcelColumnAttribute>())
            .Where(a => a is { IsMethod: true })
            .Select(a => a!.Name!)
            .ToList();
}
