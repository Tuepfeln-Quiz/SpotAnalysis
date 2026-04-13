using SpotAnalysis.Data.Models.Quizzes;

namespace SpotAnalysis.Services.DTOs;

public class ChemicalDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string Formula { get; init; }
    public required string Color { get; init; }
    public required List<MethodInfoDto> MethodInfo { get; init; }

    public static ChemicalDto FromInput(STLInput input)
    {
        return new ChemicalDto
        {
            Id = input.ChemicalID,
            Color = input.Chemical.Color,
            Name = input.Chemical.Name,
            Formula = input.Chemical.Formula,
            MethodInfo = input.Chemical.MethodOutputs.Select(mo => new MethodInfoDto
            {
                Name = mo.Method.Name,
                Color = mo.Color,
            }).ToList(),
        };
    }
}