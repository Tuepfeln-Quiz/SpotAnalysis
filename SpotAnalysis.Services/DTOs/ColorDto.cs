namespace SpotAnalysis.Services.DTOs;

public class ColorDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string HexValue { get; init; }
    public required bool IsColorless { get; init; }

    public string BorderColor => IsColorless ? "transparent" : HexValue;
}
