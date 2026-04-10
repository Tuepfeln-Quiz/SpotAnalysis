using ExcelImportExport.Helper;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace ExcelImportExport;

public static class ExcelImporter
{
    public static List<T> Import<T>(string filePath, string? sheetName = null) where T : new()
    {
        using var stream = File.OpenRead(filePath);
        return Import<T>(stream, GetFormatFromPath(filePath), sheetName);
    }

    public static List<T> Import<T>(Stream stream, ExcelFormat format, string? sheetName = null) where T : new()
    {
        var workbook = OpenWorkbook(stream, format);
        var sheet = ResolveSheet(workbook, sheetName, typeof(T));
        return ReadSheet<T>(sheet);
    }

    public static WorkbookReader Open(string filePath)
    {
        var stream = File.OpenRead(filePath);
        return Open(stream, GetFormatFromPath(filePath));
    }

    public static WorkbookReader Open(Stream stream, ExcelFormat format)
    {
        var workbook = OpenWorkbook(stream, format);
        return new WorkbookReader(workbook, stream);
    }

    public static Dictionary<string, List<T>> ImportAllSheets<T>(string filePath) where T : new()
    {
        using var stream = File.OpenRead(filePath);
        return ImportAllSheets<T>(stream, GetFormatFromPath(filePath));
    }

    public static Dictionary<string, List<T>> ImportAllSheets<T>(Stream stream, ExcelFormat format) where T : new()
    {
        var workbook = OpenWorkbook(stream, format);
        var result = new Dictionary<string, List<T>>();

        for (var i = 0; i < workbook.NumberOfSheets; i++)
        {
            var sheet = workbook.GetSheetAt(i);
            if (sheet.LastRowNum < 1) continue;
            result[sheet.SheetName] = ReadSheet<T>(sheet);
        }

        return result;
    }

    private static ISheet ResolveSheet(IWorkbook workbook, string? sheetName, Type type)
    {
        if (sheetName != null)
        {
            return workbook.GetSheet(sheetName)
                ?? throw new ArgumentException($"Sheet '{sheetName}' not found.");
        }

        // Try matching by ExcelSheetAttribute name
        var attrName = ReflectionHelper.GetSheetName(type);
        var sheet = workbook.GetSheet(attrName);
        if (sheet != null) return sheet;

        // Fallback to first sheet
        return workbook.GetSheetAt(0);
    }

    internal static List<T> ReadSheet<T>(ISheet sheet) where T : new()
    {
        var mappings = ReflectionHelper.GetPropertyMappings(typeof(T));
        var headerRow = sheet.GetRow(sheet.FirstRowNum);

        if (headerRow == null)
            return [];

        // Build column index → property mapping
        var columnMap = new Dictionary<int, ReflectionHelper.PropertyMapping>();
        for (var col = headerRow.FirstCellNum; col < headerRow.LastCellNum; col++)
        {
            var headerValue = headerRow.GetCell(col)?.StringCellValue?.Trim();
            if (string.IsNullOrEmpty(headerValue)) continue;

            var mapping = mappings.FirstOrDefault(m =>
                string.Equals(m.ColumnName, headerValue, StringComparison.OrdinalIgnoreCase));

            if (mapping != null)
                columnMap[col] = mapping;
        }

        var result = new List<T>();
        for (var rowIndex = sheet.FirstRowNum + 1; rowIndex <= sheet.LastRowNum; rowIndex++)
        {
            var row = sheet.GetRow(rowIndex);
            if (row == null) continue;

            var item = new T();
            var hasValue = false;

            foreach (var (colIndex, mapping) in columnMap)
            {
                var cell = row.GetCell(colIndex);
                if (cell == null || cell.CellType == CellType.Blank) continue;

                var value = ConvertCellValue(cell, mapping.Property.PropertyType);
                if (value != null)
                {
                    mapping.Property.SetValue(item, value);
                    hasValue = true;
                }
            }

            if (hasValue)
                result.Add(item);
        }

        return result;
    }

    private static object? ConvertCellValue(ICell cell, Type targetType)
    {
        var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

        try
        {
            return cell.CellType switch
            {
                CellType.Numeric when DateUtil.IsCellDateFormatted(cell) && cell.DateCellValue is { } dateVal => ConvertDateValue(dateVal, underlying),
                CellType.Numeric => ConvertNumericValue(cell.NumericCellValue, underlying),
                CellType.String => ConvertStringValue(cell.StringCellValue, underlying),
                CellType.Boolean => ConvertBoolValue(cell.BooleanCellValue, underlying),
                CellType.Formula => ConvertFormulaCell(cell, underlying),
                _ => null
            };
        }
        catch
        {
            return null;
        }
    }

    private static object? ConvertDateValue(DateTime dateValue, Type targetType)
    {
        if (targetType == typeof(DateTime)) return dateValue;
        if (targetType == typeof(DateOnly)) return DateOnly.FromDateTime(dateValue);
        if (targetType == typeof(string)) return dateValue.ToString("yyyy-MM-dd HH:mm:ss");
        return null;
    }

    private static object? ConvertNumericValue(double numericValue, Type targetType)
    {
        if (targetType == typeof(double)) return numericValue;
        if (targetType == typeof(float)) return (float)numericValue;
        if (targetType == typeof(decimal)) return (decimal)numericValue;
        if (targetType == typeof(int)) return (int)numericValue;
        if (targetType == typeof(long)) return (long)numericValue;
        if (targetType == typeof(string)) return numericValue.ToString();
        if (targetType == typeof(bool)) return numericValue != 0;
        return Convert.ChangeType(numericValue, targetType);
    }

    private static object? ConvertStringValue(string stringValue, Type targetType)
    {
        if (targetType == typeof(string)) return stringValue;
        if (string.IsNullOrWhiteSpace(stringValue)) return null;

        if (targetType == typeof(int) && int.TryParse(stringValue, out var i)) return i;
        if (targetType == typeof(long) && long.TryParse(stringValue, out var l)) return l;
        if (targetType == typeof(double) && double.TryParse(stringValue, out var d)) return d;
        if (targetType == typeof(float) && float.TryParse(stringValue, out var f)) return f;
        if (targetType == typeof(decimal) && decimal.TryParse(stringValue, out var dec)) return dec;
        if (targetType == typeof(bool) && bool.TryParse(stringValue, out var b)) return b;
        if (targetType == typeof(DateTime) && DateTime.TryParse(stringValue, out var dt)) return dt;
        if (targetType == typeof(DateOnly) && DateOnly.TryParse(stringValue, out var dateOnly)) return dateOnly;

        return null;
    }

    private static object? ConvertBoolValue(bool boolValue, Type targetType)
    {
        if (targetType == typeof(bool)) return boolValue;
        if (targetType == typeof(string)) return boolValue.ToString();
        return null;
    }

    private static object? ConvertFormulaCell(ICell cell, Type targetType)
    {
        return cell.CachedFormulaResultType switch
        {
            CellType.Numeric when DateUtil.IsCellDateFormatted(cell) && cell.DateCellValue is { } fDateVal => ConvertDateValue(fDateVal, targetType),
            CellType.Numeric => ConvertNumericValue(cell.NumericCellValue, targetType),
            CellType.String => ConvertStringValue(cell.StringCellValue, targetType),
            CellType.Boolean => ConvertBoolValue(cell.BooleanCellValue, targetType),
            _ => null
        };
    }

    private static IWorkbook OpenWorkbook(Stream stream, ExcelFormat format) =>
        format == ExcelFormat.Xlsx ? new XSSFWorkbook(stream) : new HSSFWorkbook(stream);

    private static ExcelFormat GetFormatFromPath(string filePath) =>
        Path.GetExtension(filePath).ToLowerInvariant() switch
        {
            ".xlsx" => ExcelFormat.Xlsx,
            ".xls" => ExcelFormat.Xls,
            _ => throw new ArgumentException($"Unsupported file extension: {Path.GetExtension(filePath)}. Use .xlsx or .xls.")
        };
}