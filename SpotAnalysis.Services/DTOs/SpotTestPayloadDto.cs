using SpotAnalysis.Data.Enums;

namespace SpotAnalysis.Services.DTOs;

public class SpotTestPayloadDto
{
    public required List<LabChemicalDto> UnknownEducts { get; init; }
    public required List<LabChemicalDto> AvailableAdditives { get; init; }
    public required List<Method> AvailableMethods { get; init; }
}
