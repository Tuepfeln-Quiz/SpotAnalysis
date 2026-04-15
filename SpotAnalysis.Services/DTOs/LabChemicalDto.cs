using SpotAnalysis.Data.Enums;

namespace SpotAnalysis.Services.DTOs;

public class LabChemicalDto
{
    public int ChemicalID { get; set; }
    public string Name { get; set; } = "";
    public string Formula { get; set; } = "";
    public string? ImagePath { get; set; }
    public ChemicalType Type { get; set; }
    public int ChemicalTypeID { get; set; }
    public string ChemicalTypeName { get; set; } = "";
    public string Color { get; set; } = "";
    public Dictionary<string, string> MethodOutputs { get; set; } = new();
}
