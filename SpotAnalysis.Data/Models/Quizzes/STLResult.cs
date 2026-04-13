namespace SpotAnalysis.Data.Models.Quizzes;

/// <summary>
/// Represents the results for the specified SpotTestLight Questions. 
/// The user attempting the question can be tracked with the Attempt Object.
/// </summary>
public class STLResult {
    [Key]
    public int ResultID { get; set; }
    public int AttemptID { get; set; }
    public int QuestionID { get; set; }
    public int ChosenReactionID { get; set; }
    public bool IsCorrect { get; set; }


    [ForeignKey(nameof(AttemptID))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public QuizAttempt Attempt { get; set; } = null!;

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Question Question { get; set; } = null!;

    [ForeignKey(nameof(ChosenReactionID))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Reaction ChosenReaction { get; set; } = null!;
}
