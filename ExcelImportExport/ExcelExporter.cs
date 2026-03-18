using System.Collections;
using ExcelImportExport.Helper;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace ExcelImportExport;

public static class ExcelExporter
{
    public static void Export<T>(IEnumerable<T> data, string filePath)
    {
        var format = GetFormatFromPath(filePath);
        using var stream = File.Create(filePath);
        var workbook = CreateWorkbook(format);
        var sheetName = ReflectionHelper.GetSheetName(typeof(T));
        WriteSheet(workbook, sheetName, data, typeof(T));
        workbook.Write(stream, leaveOpen: true);
    }

    public static void ExportMultiSheet(string filePath, params SheetData[] sheets)
    {
        var format = GetFormatFromPath(filePath);
        var workbook = CreateWorkbook(format);

        foreach (var sheet in sheets)
        {
            WriteSheet(workbook, sheet.SheetName, sheet.Data, sheet.ItemType);
        }

        using var stream = File.Create(filePath);
        workbook.Write(stream, leaveOpen: true);
    }

    private static void WriteSheet(IWorkbook workbook, string sheetName, IEnumerable data, Type itemType)
    {
        var sheet = workbook.CreateSheet(sheetName);
        var mappings = ReflectionHelper.GetPropertyMappings(itemType);

        // Header row
        var headerRow = sheet.CreateRow(0);
        for (var i = 0; i < mappings.Count; i++)
        {
            headerRow.CreateCell(i).SetCellValue(mappings[i].ColumnName);
        }

        // Data rows
        var rowIndex = 1;
        foreach (var item in data)
        {
            var row = sheet.CreateRow(rowIndex++);
            for (var i = 0; i < mappings.Count; i++)
            {
                var cell = row.CreateCell(i);
                var value = mappings[i].Property.GetValue(item);
                SetCellValue(cell, value, workbook);
            }
        }
    }

    private static void SetCellValue(ICell cell, object? value, IWorkbook workbook)
    {
        switch (value)
        {
            case null:
                cell.SetCellType(CellType.Blank);
                break;
            case string s:
                cell.SetCellValue(s);
                break;
            case int i:
                cell.SetCellValue(i);
                break;
            case long l:
                cell.SetCellValue(l);
                break;
            case double d:
                cell.SetCellValue(d);
                break;
            case float f:
                cell.SetCellValue(f);
                break;
            case decimal dec:
                cell.SetCellValue((double)dec);
                break;
            case bool b:
                cell.SetCellValue(b);
                break;
            case DateTime dt:
                cell.SetCellValue(dt);
                var style = workbook.CreateCellStyle();
                style.DataFormat = workbook.CreateDataFormat().GetFormat("yyyy-MM-dd HH:mm:ss");
                cell.CellStyle = style;
                break;
            case DateOnly dateOnly:
                cell.SetCellValue(dateOnly.ToDateTime(TimeOnly.MinValue));
                var dateStyle = workbook.CreateCellStyle();
                dateStyle.DataFormat = workbook.CreateDataFormat().GetFormat("yyyy-MM-dd");
                cell.CellStyle = dateStyle;
                break;
            default:
                cell.SetCellValue(value.ToString());
                break;
        }
    }

    private static IWorkbook CreateWorkbook(ExcelFormat format) =>
        format == ExcelFormat.Xlsx ? new XSSFWorkbook() : new HSSFWorkbook();

    private static ExcelFormat GetFormatFromPath(string filePath) =>
        Path.GetExtension(filePath).ToLowerInvariant() switch
        {
            ".xlsx" => ExcelFormat.Xlsx,
            ".xls" => ExcelFormat.Xls,
            _ => throw new ArgumentException($"Unsupported file extension: {Path.GetExtension(filePath)}. Use .xlsx or .xls.")
        };
}
