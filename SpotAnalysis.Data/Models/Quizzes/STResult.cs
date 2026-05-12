namespace SpotAnalysis.Data.Models.Quizzes;

/// <summary>
/// Represents the mapping between a quiz attempt and a question. The ResultID is used to distinguish between different questions in the STChemicalResult table.
/// </summary>
public class StResult
{
    [Key] public int ResultId { get; set; }

    public int AttemptId { get; set; }
    public int QuestionId { get; set; }


    [ForeignKey(nameof(AttemptId))]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public QuizAttempt Attempt { get; set; } = null!;

    [ForeignKey(nameof(QuestionId))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Question Question { get; set; } = null!;

    public virtual ICollection<StChemicalResult> ChemicalResults { get; set; } = [];
}
