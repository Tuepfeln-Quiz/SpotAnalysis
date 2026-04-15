using SpotAnalysis.Data.Models.Quizzes;

namespace SpotAnalysis.Services.DTOs;

public class STLQuestionDto
{
    public required int Id { get; init; }
    public required string Description { get; init; }
    public required int Order { get; init; }
    public required ChemicalDto Educt { get; init; }
    public required string Observation { get; init; }

    public static STLQuestionDto FromQuestion(QuizQuestion question)
    {
        return new STLQuestionDto
        {
            Id = question.QuestionID,
            Description = question.Question.Description,
            Order = question.Order,
            Educt = ChemicalDto.FromInput(question.Question.STLInput!.Chemical1),
            Observation = question.Question.STLInput.Observation.Description
        };
    }
}