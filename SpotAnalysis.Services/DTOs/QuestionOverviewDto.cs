using SpotAnalysis.Data.Enums;

namespace SpotAnalysis.Services.DTOs;

public class QuestionOverviewDto
{
    public required int Id { get; set; }
    public required string Description { get; set; }
    public required QuestionType Type { get; set; }
    public string? CreatedByName { get; set; }
    public int ChemicalCount { get; set; }
    public int MethodCount { get; set; }
    public int ReactionCount { get; set; }
    public int QuizCount { get; set; }
}
