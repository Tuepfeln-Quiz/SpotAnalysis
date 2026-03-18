using System.Collections;

namespace ExcelImportExport.Helper;

public record SheetData(string SheetName, IEnumerable Data, Type ItemType)
{
    public static SheetData From<T>(IEnumerable<T> data, string? sheetName = null)
    {
        var name = sheetName ?? ReflectionHelper.GetSheetName(typeof(T));
        return new SheetData(name, data, typeof(T));
    }
}
