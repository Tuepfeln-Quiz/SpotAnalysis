namespace SpotAnalysis.Data.Models.Quizzes;

/// <summary>
/// Represents the results for the specified SpotTestLight Questions. 
/// The user attempting the question can be tracked with the Attempt Object.
/// </summary>
public class StlResult
{
    [Key] public int ResultId { get; set; }

    public int AttemptId { get; set; }
    public int QuestionId { get; set; }
    public int ChosenReactionId { get; set; }
    public bool IsCorrect { get; set; }


    [ForeignKey(nameof(AttemptId))]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public QuizAttempt Attempt { get; set; } = null!;

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Question Question { get; set; } = null!;

    [ForeignKey(nameof(ChosenReactionId))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Reaction ChosenReaction { get; set; } = null!;
}
