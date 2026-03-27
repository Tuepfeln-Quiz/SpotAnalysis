namespace SpotAnalysis.Services.DTOs;

public class QuizDto
{
    public required string Name { get; init; }
    public List<STQuestionDto> STQuestions { get; init; }
    public List<STLQuestionDto> STLQuestions { get; init; }
}