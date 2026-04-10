using SpotAnalysis.Data.Enums;

namespace SpotAnalysis.Services.DTOs;

public class QuestionOverviewDto
{
    public required int Id { get; set; }
    public required string Description { get; set; }
    public required QuestionType Type { get; set; }
}