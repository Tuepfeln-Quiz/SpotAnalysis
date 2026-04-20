using SpotAnalysis.Data.Enums;

namespace SpotAnalysis.Services.DTOs;

public class QuizQuestionPayloadDto
{
    public required int QuestionID { get; init; }
    public required int Order { get; init; }
    public required string Description { get; init; }
    public required QuestionType Type { get; init; }

    public SpotTestPayloadDto? SpotTest { get; init; }
    public LightPayloadDto? Light { get; init; }
}
