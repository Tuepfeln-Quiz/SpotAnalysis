namespace SpotAnalysis.Services.DTOs;

public class UpdateQuizDto
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required List<QuestionDto> Questions { get; set; }
}