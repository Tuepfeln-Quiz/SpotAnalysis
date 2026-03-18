using ExcelImportExport.Attributes;
using System.Reflection;

namespace ExcelImportExport.Helper;

internal static class ReflectionHelper
{
    internal static string GetSheetName(Type type)
    {
        var attr = type.GetCustomAttribute<ExcelSheetAttribute>();
        return attr?.Name ?? type.Name;
    }

    internal static List<PropertyMapping> GetPropertyMappings(Type type)
    {
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite);

        var mappings = new List<PropertyMapping>();

        foreach (var prop in properties)
        {
            var attr = prop.GetCustomAttribute<ExcelColumnAttribute>();

            if (attr?.Ignore == true)
                continue;

            var columnName = attr?.Name ?? prop.Name;
            var order = attr?.Order ?? int.MaxValue;

            mappings.Add(new PropertyMapping(prop, columnName, order));
        }

        return mappings.OrderBy(m => m.Order).ThenBy(m => m.ColumnName).ToList();
    }

    internal record PropertyMapping(PropertyInfo Property, string ColumnName, int Order);
}
