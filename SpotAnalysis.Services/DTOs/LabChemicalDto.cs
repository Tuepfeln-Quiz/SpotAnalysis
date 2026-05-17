using SpotAnalysis.Data.Enums;

namespace SpotAnalysis.Services.DTOs;

public class LabChemicalDto
{
    public int ChemicalId { get; set; }
    public string Name { get; set; } = "";
    public string Formula { get; set; } = "";
    public string? ImagePath { get; set; }
    public ChemicalType Type { get; set; }
    public ColorDto Color { get; set; } = null!;
    public Dictionary<Method, ColorDto> MethodOutputs { get; set; } = new();
}
