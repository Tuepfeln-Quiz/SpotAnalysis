using ExcelImportExport.Helper;
using SpotAnalysis.Services.DTOs;

namespace SpotAnalysis.Services.Services;

public interface IXlsImportExportService
{
    Task<ImportResult> ImportFromFileAsync(string filePath);
    Task<ImportResult> ImportFromStreamAsync(Stream stream, ExcelFormat format);
    Task ExportToFileAsync(string filePath);
    Task ExportToStreamAsync(Stream stream, ExcelFormat format);
}
