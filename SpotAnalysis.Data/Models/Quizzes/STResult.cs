namespace SpotAnalysis.Data.Models.Quizzes;

/// <summary>
/// Represents the mapping between a quiz attempt and a question. The ResultID is used to distinguish between different questions in the STChemicalResult table.
/// </summary>

public class STResult {
    [Key]
    public int ResultID { get; set; }
    public int AttemptID { get; set; }
    public int QuestionID { get; set; }


    [ForeignKey(nameof(AttemptID))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public QuizAttempt Attempt { get; set; } = null!;

    [ForeignKey(nameof(QuestionID))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Question Question { get; set; } = null!;

    public virtual ICollection<STChemicalResult> ChemicalResults { get; set; } = [];
}