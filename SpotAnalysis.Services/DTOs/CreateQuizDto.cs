namespace SpotAnalysis.Services.DTOs;

public class CreateQuizDto
{
    public required string Name { get; set; }
    public required List<QuestionDto> Questions { get; set; }
}