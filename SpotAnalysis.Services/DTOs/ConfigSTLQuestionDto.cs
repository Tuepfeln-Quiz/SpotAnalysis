namespace SpotAnalysis.Services.DTOs;

public class ConfigSTLQuestionDto
{
    public required string Description { get; set; }
    public required int ObservationId { get; set; }
    public required int ChemicalId { get; set; }
    public required List<int> AvailableReactions { get; set; }
}