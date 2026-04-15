namespace SpotAnalysis.Services.DTOs;

public class ConfigSTLQuestionDto
{
    public int? Id { get; set; }
    public required string Description { get; set; }
    public required int ReactionId { get; set; }
    public required int ShowEductId { get; set; }
    public required List<int> AvailableReactions { get; set; }
    public required string Title  { get; set; }
}
