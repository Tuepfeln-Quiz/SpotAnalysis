using NPOI.SS.UserModel;

namespace ExcelImportExport.Helper;

public sealed class WorkbookReader : IDisposable
{
    private readonly IWorkbook _workbook;
    private readonly Stream _stream;

    internal WorkbookReader(IWorkbook workbook, Stream stream)
    {
        _workbook = workbook;
        _stream = stream;
    }

    public List<T> ReadSheet<T>(string? sheetName = null) where T : new()
    {
        var sheet = ResolveSheet(sheetName, typeof(T));
        return ExcelImporter.ReadSheet<T>(sheet);
    }

    public IReadOnlyList<string> SheetNames
    {
        get
        {
            var names = new List<string>();
            for (var i = 0; i < _workbook.NumberOfSheets; i++)
                names.Add(_workbook.GetSheetAt(i).SheetName);
            return names;
        }
    }

    private ISheet ResolveSheet(string? sheetName, Type type)
    {
        if (sheetName != null)
        {
            return _workbook.GetSheet(sheetName)
                ?? throw new ArgumentException($"Sheet '{sheetName}' not found.");
        }

        var attrName = ReflectionHelper.GetSheetName(type);
        var sheet = _workbook.GetSheet(attrName);
        if (sheet != null) return sheet;

        return _workbook.GetSheetAt(0);
    }

    public void Dispose()
    {
        _workbook.Close();
        _stream.Dispose();
    }
}
