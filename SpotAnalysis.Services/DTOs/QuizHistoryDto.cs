using SpotAnalysis.Data.Enums;

namespace SpotAnalysis.Services.DTOs;

public class QuizHistoryDto
{
    public int AttemptId { get; set; }
    public int QuizId { get; set; }
    public string QuizName { get; set; } = "";
    public QuestionType QuizType { get; set; }
    public DateTime Started { get; set; }
    public DateTime? Completed { get; set; }
    public int CorrectAnswers { get; set; }
    public int TotalQuestions { get; set; }
    public bool IsCompleted => Completed.HasValue;
}