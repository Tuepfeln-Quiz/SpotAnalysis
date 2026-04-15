using SpotAnalysis.Data.Enums;

namespace SpotAnalysis.Services.DTOs;

public class QuestionDetailDto
{
    public required int Id { get; init; }
    public required string Description { get; init; }
    public required QuestionType Type { get; init; }
    
    /// <summary>
    /// ST specific
    /// </summary>
    public List<ChemicalQuestionDto> Chemicals { get; init; } = [];
    /// <summary>
    /// ST specific
    /// </summary>
    public List<MethodQuestionDto> Methods { get; init; } = [];
    /// <summary>
    /// STL specific
    /// </summary>
    public int ReactionId { get; init; }
    /// <summary>
    /// STL specific
    /// </summary>
    public List<int> AvailableReactionIds { get; init; } = [];
}
