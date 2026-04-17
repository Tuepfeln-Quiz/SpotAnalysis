using SpotAnalysis.Data.Enums;

namespace SpotAnalysis.Services.DTOs;

public class QuestionOverviewDto
{
    public required int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required QuestionType Type { get; set; }
    public Guid? CreatedById { get; set; }
    public string? CreatedByName { get; set; }
    public int QuizCount { get; set; }
    
    /// <summary>
    /// ST Specific
    /// </summary>
    public int ChemicalCount { get; set; }
    /// <summary>
    /// ST Specific
    /// </summary>
    public int MethodCount { get; set; }
    /// <summary>
    /// STL Specific
    /// </summary>
    public int ReactionCount { get; set; }
}
