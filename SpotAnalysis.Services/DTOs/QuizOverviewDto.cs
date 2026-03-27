namespace SpotAnalysis.Services.DTOs;

public class QuizOverviewDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public int STCount { get; init; }
    public int STLCount { get; init; }
}