namespace SpotAnalysis.Services.DTOs;

public class ValAndSaveQuestionDto
{
    public required Guid UserId { get; set; }
    public required int QuizId { get; set; }
    public required int QuestionId { get; set; }
    public required string ChosenFormula { get; set; }
    public required int CorrectChemicalId { get; set; }
}