using SpotAnalysis.Data.Enums;

namespace SpotAnalysis.Services.DTOs;

public class MethodOutputEntry
{
    public Method Method { get; set; }
    public int ColorId { get; set; }
    public string ColorName { get; set; } = "";
}
