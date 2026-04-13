using SpotAnalysis.Data.Models;
using SpotAnalysis.Data.Models.Quizzes;

namespace SpotAnalysis.Services.DTOs;

public class ChemicalDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string Formula { get; init; }
    public required string Color { get; init; }
    public required List<MethodInfoDto> MethodInfo { get; init; }

    public static ChemicalDto FromInput(Chemical input)
    {
        return new ChemicalDto
        {
            Id = input.ChemicalID,
            Color = input.Color,
            Name = input.Name,
            Formula = input.Formula,
            MethodInfo = input.MethodOutputs.Select(mo => new MethodInfoDto
            {
                Name = mo.Method.Name,
                Color = mo.Color,
            }).ToList(),
        };
    }
}