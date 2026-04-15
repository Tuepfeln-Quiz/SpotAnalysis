using SpotAnalysis.Data.Enums;

namespace SpotAnalysis.Services.DTOs;

public class QuestionDetailDto
{
    public required int Id { get; init; }
    public required string Description { get; init; }
    public required QuestionType Type { get; init; }
    public List<ChemicalQuestionDto> Chemicals { get; init; } = [];
    public List<MethodQuestionDto> Methods { get; init; } = [];
    public int ReactionId { get; init; }
    public List<int> AvailableReactionIds { get; init; } = [];
}
