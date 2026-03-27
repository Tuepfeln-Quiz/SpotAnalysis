namespace SpotAnalysis.Services.DTOs;

public class ChemicalQuestionDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string Formula { get; init; }
    public required string Color { get; init; }
    public required bool IsAdditive { get; init; }
}