using NPOI.SS.UserModel;

namespace ExcelImportExport.Helper;

public sealed class WorkbookReader : IDisposable
{
    private readonly Stream _stream;
    private readonly IWorkbook _workbook;

    internal WorkbookReader(IWorkbook workbook, Stream stream)
    {
        _workbook = workbook;
        _stream = stream;
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

    public void Dispose()
    {
        _workbook.Close();
        _stream.Dispose();
    }

    public List<T> ReadSheet<T>(string? sheetName = null) where T : new()
    {
        var sheet = ExcelImporter.ResolveSheet(_workbook, sheetName, typeof(T));
        return ExcelImporter.ReadSheet<T>(sheet);
    }
}
