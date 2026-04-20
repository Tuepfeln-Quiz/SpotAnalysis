namespace SpotAnalysis.Services.DTOs;

public class GlobalStatisticsDto
{
    public int TotalUsers { get; set; }
    public int TotalTeachers { get; set; }
    public int TotalStudents { get; set; }
    public int TotalGroups { get; set; }
    public int TotalQuizzes { get; set; }
    public int TotalAttempts { get; set; }
    public int TotalCompletedAttempts { get; set; }
    public double AverageScorePercent { get; set; }
    public int TotalQuestionsAnswered { get; set; }
    public int TotalCorrectAnswers { get; set; }
}