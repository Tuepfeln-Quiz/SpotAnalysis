namespace SpotAnalysis.Services.DTOs;

public class LightPayloadDto
{
    public required ChemicalDto ShownEduct { get; init; }
    public required string Observation { get; init; }
    public required int CorrectReactionID { get; init; }
    public required List<LabReactionDto> AvailableReactions { get; init; }
}
