namespace SpotAnalysis.Services.DTOs;

public class STLQuestionDto
{
    public required int Id { get; init; }
    public required string Description { get; init; }
    public required int Order { get; init; }
    public required ChemicalDto Educt { get; init; }
    public required string Observation { get; init; }
}