namespace SpotAnalysis.Services.DTOs;

public class STQuestionDto
{
    public required int Id { get; init; }
    public required string Description { get; init; }
    public required int Order { get; init; }
    public List<ChemicalQuestionDto> Chemicals { get; init; }
    public List<MethodQuestionDto> Methods { get; init; }
}