namespace ExcelImportExport.Helper;

public enum ExcelFormat
{
    Xlsx,
    Xls
}

internal static class ExcelFormatHelper
{
    internal static ExcelFormat FromPath(string filePath) =>
        Path.GetExtension(filePath).ToLowerInvariant() switch
        {
            ".xlsx" => ExcelFormat.Xlsx,
            ".xls" => ExcelFormat.Xls,
            _ => throw new ArgumentException(
                $"Unsupported file extension: {Path.GetExtension(filePath)}. Use .xlsx or .xls.")
        };
}
