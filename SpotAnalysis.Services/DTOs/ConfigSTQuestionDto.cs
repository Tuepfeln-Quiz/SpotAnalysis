namespace SpotAnalysis.Services.DTOs;

public class ConfigSTQuestionDto
{
    public required string Description { get; set; }
    public required List<int> AvailableChemicals { get; set; }
    public required List<int> AvailableMethods { get; set; }
}