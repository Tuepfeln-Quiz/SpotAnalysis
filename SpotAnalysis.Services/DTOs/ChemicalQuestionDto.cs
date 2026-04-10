using SpotAnalysis.Data.Enums;
using SpotAnalysis.Data.Models;
using SpotAnalysis.Data.Models.Quizzes;

namespace SpotAnalysis.Services.DTOs;

public class ChemicalQuestionDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string Formula { get; init; }
    public required string Color { get; init; }
    public required bool IsAdditive { get; init; }

    public static ChemicalQuestionDto FromAvailable(STAvailableChemical availableChemical)
    {
        return new ChemicalQuestionDto
        {
            Id = availableChemical.ChemicalID,
            Color = availableChemical.Chemical.Color,
            Name = availableChemical.Chemical.Name,
            Formula = availableChemical.Chemical.Formula,
            IsAdditive = availableChemical.Chemical.Type == ChemicalType.Additive,
        };
    }
}