using SpotAnalysis.Data.Models.Quizzes;

namespace SpotAnalysis.Services.DTOs;

public class STLQuestionDto
{
    public required int Id { get; init; }
    public required string Description { get; init; }
    public required int Order { get; init; }
    public required ChemicalDto Educt { get; init; }
    public required string Observation { get; init; }

    public static STLQuestionDto FromQuestion(Question question)
    {
        return new STLQuestionDto
        {
            Id = question.QuestionID,
            Description = question.Description,
            Order = 1, //TODO: use correct order number
            Educt = ChemicalDto.FromInput(question.STLInputs.ElementAt(0)),
            Observation = question.STLInputs.ElementAt(0).Observation.Description
        };
    }
}