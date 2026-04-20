using SpotAnalysis.Data.Enums;

namespace SpotAnalysis.Services.DTOs;

public class DetailedQuizHistoryDto : QuizHistoryDto
{
    public List<QuestionResultDto> QuestionResults { get; set; } = new();
}

public class QuestionResultDto
{
    public int QuestionId { get; set; }
    public string QuestionTitle { get; set; } = "";
    public QuestionType QuestionType { get; set; }
    public int TotalSubQuestions { get; set; }
    public int CorrectSubQuestions { get; set; }
    public bool IsFullyCorrect => CorrectSubQuestions == TotalSubQuestions;
}