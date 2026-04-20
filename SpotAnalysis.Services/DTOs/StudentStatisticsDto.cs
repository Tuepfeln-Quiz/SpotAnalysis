namespace SpotAnalysis.Services.DTOs;

public class StudentStatisticsDto
{
    public required Guid StudentId { get; init; }
    public required string UserName { get; init; }
    public int TotalAttempts { get; init; }
    public int TotalCorrect { get; init; }
    public int TotalQuestions { get; init; }
    public DateTime? LastAttemptAt { get; init; }
    public double AveragePercent => TotalQuestions > 0 ? (TotalCorrect * 100.0 / TotalQuestions) : 0;
}