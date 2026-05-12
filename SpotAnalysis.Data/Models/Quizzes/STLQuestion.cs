namespace SpotAnalysis.Data.Models.Quizzes;

/// <summary>
/// Contains the input for a SpotTestLight question, which is the chemical of type educt shown to the quiztaker and the expected reaction. The reaction contains the Observation (also visible to quiztaker) as well as the second chemical the student needs to identify.
/// </summary>
public class StlQuestion
{
    [Key] [ForeignKey(nameof(QuestionId))] public int QuestionId { get; set; }

    public int ReactionId { get; set; }
    public int ShownEductId { get; set; }

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public virtual Question Question { get; set; } = null!;

    [ForeignKey(nameof(ReactionId))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public virtual Reaction Reaction { get; set; } = null!;

    [ForeignKey(nameof(ShownEductId))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public virtual Chemical ShownEduct { get; set; } = null!;

    public virtual ICollection<StlAvailableReaction> AvailableReactions { get; set; } = [];
}
