using SpotAnalysis.Data.Models;

namespace SpotAnalysis.Services.DTOs;


public class ValidateQuestionDto
{
    public required Guid UserId { get; set; }
    public required int QuizId { get; set; }
    public required int QuestionId { get; set; }
}

/// <summary>
/// 
/// </summary>
public class ValidateStQuestionDto : ValidateQuestionDto
{
    public required List<string> ChemicalFormulas { get; set; } = [];
}

/// <summary>
/// to verify an STL answer, check if the observation in the question's STLInput matches the provided Reaction's observation
/// </summary>
public class ValidateStlQuestionDto : ValidateQuestionDto
{
    public required int ReactionId { get; set; }
}