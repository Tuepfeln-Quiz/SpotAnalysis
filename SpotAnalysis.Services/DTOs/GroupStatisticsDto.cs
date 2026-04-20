namespace SpotAnalysis.Services.DTOs;

public class GroupStatisticsDto
{
    public int GroupId { get; set; }
    public string GroupName { get; set; } = "";
    public string? Description { get; set; }
    public int TotalUsers { get; set; }
    public int TotalQuizzes { get; set; }
    public int TotalAttempts { get; set; }
    public int TotalCompletedAttempts { get; set; }
    public double AverageScorePercent { get; set; }
    public List<UserInGroupStatisticsDto> UserStatistics { get; set; } = new();
}

public class UserInGroupStatisticsDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = "";
    public int TotalAttempts { get; set; }
    public int TotalCompletedAttempts { get; set; }
    public double AverageScorePercent { get; set; }
}