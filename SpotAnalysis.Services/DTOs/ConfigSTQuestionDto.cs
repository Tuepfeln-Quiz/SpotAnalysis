using SpotAnalysis.Data.Enums;

namespace SpotAnalysis.Services.DTOs;

public class ConfigStQuestionDto
{
    public int? Id { get; set; }
    public required string Description { get; set; }
    public required List<int> AvailableChemicals { get; set; }
    public required List<Method> AvailableMethods { get; set; }
    public required string Title { get; set; }
}
