using ExcelImportExport.Helper;

namespace SpotAnalysis.Services.Services;

public interface IXlsImportExportService
{
    Task ImportFromFileAsync(string filePath);
    Task ImportFromStreamAsync(Stream stream, ExcelFormat format);
    Task ExportToFileAsync(string filePath);
    Task ExportToStreamAsync(Stream stream, ExcelFormat format);
}
