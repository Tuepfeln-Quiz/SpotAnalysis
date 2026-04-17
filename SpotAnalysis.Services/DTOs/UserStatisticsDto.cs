namespace SpotAnalysis.Services.DTOs;


public class UserStatisticsDto
{
    public int TotalAttempts { get; set; }
    public int LightAttempts { get; set; }
    public int TuepfelnAttempts { get; set; }
    public int TotalCorrect { get; set; }
    public int TotalQuestions { get; set; }
    public double AveragePercent => TotalQuestions > 0 ? (TotalCorrect * 100.0 / TotalQuestions) : 0;
}