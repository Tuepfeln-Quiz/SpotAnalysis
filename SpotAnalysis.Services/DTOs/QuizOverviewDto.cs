namespace SpotAnalysis.Services.DTOs;

public class QuizOverviewDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required int STCount { get; init; }
    public required int STLCount { get; init; }
}