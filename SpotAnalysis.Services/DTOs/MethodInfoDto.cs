using SpotAnalysis.Data.Enums;

namespace SpotAnalysis.Services.DTOs;

public class MethodInfoDto
{
    public required Method Method { get; init; }
    public required ColorDto Color { get; init; }
}
