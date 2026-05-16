namespace SpotAnalysis.Data.Models.Quizzes;

/// <summary>
/// Contains the reactions that can be used in the specified SpotTestQuestion.
/// </summary>
[PrimaryKey(nameof(QuestionId), nameof(ReactionId))]
public class StlAvailableReaction
{
    public int QuestionId { get; set; }
    public int ReactionId { get; set; }

    [ForeignKey(nameof(QuestionId))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public StlQuestion StlQuestion { get; set; } = null!;

    [ForeignKey(nameof(ReactionId))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Reaction Reaction { get; set; } = null!;
}
