namespace SpotAnalysis.Services.DTOs;

public class QuizOverviewDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required int STCount { get; init; }
    public required int STLCount { get; init; }
    public int GroupCount { get; init; }
    public int QuestionCount { get; init; }
    public LastAttemptStatus LastAttemptStatus { get; init; } = LastAttemptStatus.NotStarted;
    public DateTime? LastCompletedAt { get; init; }
    public int TotalAttempts { get; init; }
    public double BestScorePercent { get; init; }
    public double AverageScorePercent { get; init; }
}
