namespace SpotAnalysis.Services.DTOs;

public class QuizPlayDto
{
    public required int QuizID { get; init; }
    public required string Name { get; init; }
    public required int AttemptID { get; init; }
    public required List<QuizQuestionPayloadDto> Questions { get; init; }
}
