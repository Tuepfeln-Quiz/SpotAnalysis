using SpotAnalysis.Data.Models.Quizzes;

namespace SpotAnalysis.Services.DTOs;

public class STQuestionDto
{
    public required int Id { get; init; }
    public required string Description { get; init; }
    public required int Order { get; init; }
    public List<ChemicalQuestionDto> Chemicals { get; init; }
    public List<MethodQuestionDto> Methods { get; init; }

    public static STQuestionDto FromQuestion(QuizQuestion question)
    {
        return new STQuestionDto
        {
            Id = question.QuestionID,
            Description = question.Question.Description,
            Order = question.Order,
            Chemicals = question.Question.STAvailableChemicals.Select(ChemicalQuestionDto.FromAvailable).ToList(),
            Methods = question.Question.STAvailableMehtods.Select(am => new MethodQuestionDto
            {
                Name = am.Method.Name,
                Id = am.MethodID,
            }).ToList(),
        };
    }
}