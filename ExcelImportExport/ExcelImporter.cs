using System.Globalization;
using ExcelImportExport.Helper;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace ExcelImportExport;

public static class ExcelImporter
{
    public static WorkbookReader Open(string filePath)
    {
        var stream = File.OpenRead(filePath);
        return Open(stream, ExcelFormatHelper.FromPath(filePath));
    }

    public static WorkbookReader Open(Stream stream, ExcelFormat format)
    {
        var workbook = OpenWorkbook(stream, format);
        return new WorkbookReader(workbook, stream);
    }

    internal static ISheet ResolveSheet(IWorkbook workbook, string? sheetName, Type type)
    {
        if (sheetName != null)
        {
            return workbook.GetSheet(sheetName)
                   ?? throw new ArgumentException($"Sheet '{sheetName}' not found.");
        }

        var attrName = ReflectionHelper.GetSheetName(type);
        var sheet = workbook.GetSheet(attrName);
        return sheet ?? workbook.GetSheetAt(0);
    }

    internal static List<T> ReadSheet<T>(ISheet sheet) where T : new()
    {
        var mappings = ReflectionHelper.GetPropertyMappings(typeof(T));
        var headerRow = sheet.GetRow(sheet.FirstRowNum);

        if (headerRow == null)
        {
            return [];
        }

        var columnMap = new Dictionary<int, ReflectionHelper.PropertyMapping>();
        for (var col = headerRow.FirstCellNum; col < headerRow.LastCellNum; col++)
        {
            var headerValue = headerRow.GetCell(col)?.StringCellValue?.Trim();
            if (string.IsNullOrEmpty(headerValue))
            {
                continue;
            }

            var mapping = mappings.FirstOrDefault(m =>
                string.Equals(m.ColumnName, headerValue, StringComparison.OrdinalIgnoreCase));

            if (mapping != null)
            {
                columnMap[col] = mapping;
            }
        }

        var result = new List<T>();
        for (var rowIndex = sheet.FirstRowNum + 1; rowIndex <= sheet.LastRowNum; rowIndex++)
        {
            var row = sheet.GetRow(rowIndex);
            if (row == null)
            {
                continue;
            }

            var item = new T();
            var hasValue = false;

            foreach (var (colIndex, mapping) in columnMap)
            {
                var cell = row.GetCell(colIndex);
                if (cell == null || cell.CellType == CellType.Blank)
                {
                    continue;
                }

                var value = ReadCellAsString(cell);
                if (value == null)
                {
                    continue;
                }

                mapping.Property.SetValue(item, value);
                hasValue = true;
            }

            if (hasValue)
            {
                result.Add(item);
            }
        }

        return result;
    }

    private static string? ReadCellAsString(ICell cell)
    {
        return cell.CellType switch
        {
            CellType.String => cell.StringCellValue,
            CellType.Numeric when DateUtil.IsCellDateFormatted(cell) && cell.DateCellValue is { } date
                => date.ToString("yyyy-MM-dd"),
            CellType.Numeric => FormatNumeric(cell.NumericCellValue),
            CellType.Boolean => cell.BooleanCellValue.ToString(),
            CellType.Formula => ReadFormulaCellAsString(cell),
            _ => null
        };
    }

    private static string? ReadFormulaCellAsString(ICell cell)
    {
        return cell.CachedFormulaResultType switch
        {
            CellType.String => cell.StringCellValue,
            CellType.Numeric when DateUtil.IsCellDateFormatted(cell) && cell.DateCellValue is { } date
                => date.ToString("yyyy-MM-dd"),
            CellType.Numeric => FormatNumeric(cell.NumericCellValue),
            CellType.Boolean => cell.BooleanCellValue.ToString(),
            _ => null
        };
    }

    private static string FormatNumeric(double value) =>
        Math.Floor(value) % 1 == 0 ? ((long)value).ToString() : value.ToString(CultureInfo.InvariantCulture);

    private static IWorkbook OpenWorkbook(Stream stream, ExcelFormat format) =>
        format == ExcelFormat.Xlsx ? new XSSFWorkbook(stream) : new HSSFWorkbook(stream);
}
