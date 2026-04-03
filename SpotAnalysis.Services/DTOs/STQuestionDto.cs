using SpotAnalysis.Data.Models.Quizzes;

namespace SpotAnalysis.Services.DTOs;

public class STQuestionDto
{
    public required int Id { get; init; }
    public required string Description { get; init; }
    public required int Order { get; init; }
    public List<ChemicalQuestionDto> Chemicals { get; init; }
    public List<MethodQuestionDto> Methods { get; init; }

    public static STQuestionDto FromQuestion(Question question)
    {
        return new STQuestionDto
        {
            Id = question.QuestionID,
            Description = question.Description,
            Order = 1, //TODO: use correct order number
            Chemicals = question.STAvailableChemicals.Select(ChemicalQuestionDto.FromAvailable).ToList(),
            Methods = question.STAvailableMehtods.Select(am => new MethodQuestionDto
            {
                Name = am.Method.Name,
                Id = am.MethodID,
            }).ToList(),
        };
    }
}