using System.Collections;
using ExcelImportExport.Helper;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace ExcelImportExport;

public static class ExcelExporter
{
    public static void ExportMultiSheet(string filePath, params SheetData[] sheets)
    {
        using var stream = File.Create(filePath);
        ExportMultiSheet(stream, ExcelFormatHelper.FromPath(filePath), sheets);
    }

    public static void ExportMultiSheet(Stream stream, ExcelFormat format, params SheetData[] sheets)
    {
        var workbook = CreateWorkbook(format);

        foreach (var sheet in sheets)
        {
            WriteSheet(workbook, sheet.SheetName, sheet.Data, sheet.ItemType);
        }

        workbook.Write(stream, leaveOpen: true);
    }

    private static void WriteSheet(IWorkbook workbook, string sheetName, IEnumerable data, Type itemType)
    {
        var sheet = workbook.CreateSheet(sheetName);
        var mappings = ReflectionHelper.GetPropertyMappings(itemType);

        var headerRow = sheet.CreateRow(0);
        for (var i = 0; i < mappings.Count; i++)
        {
            headerRow.CreateCell(i).SetCellValue(mappings[i].ColumnName);
        }

        var rowIndex = 1;
        foreach (var item in data)
        {
            var row = sheet.CreateRow(rowIndex++);
            for (var i = 0; i < mappings.Count; i++)
            {
                if (mappings[i].Property.GetValue(item) is string value)
                {
                    row.CreateCell(i).SetCellValue(value);
                }
            }
        }
    }

    private static IWorkbook CreateWorkbook(ExcelFormat format) =>
        format == ExcelFormat.Xlsx ? new XSSFWorkbook() : new HSSFWorkbook();
}
