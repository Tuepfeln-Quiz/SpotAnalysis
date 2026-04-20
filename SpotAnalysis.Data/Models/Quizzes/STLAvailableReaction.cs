namespace SpotAnalysis.Data.Models.Quizzes;

/// <summary>
/// Contains the reactions that can be used in the specified SpotTestQuestion.
/// </summary>

[PrimaryKey(nameof(QuestionID), nameof(ReactionID))]
public class STLAvailableReaction {
    public int QuestionID { get; set; }
    public int ReactionID { get; set; }
    
    [ForeignKey(nameof(QuestionID))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public STLQuestion STLQuestion { get; set; } = null!;

    [ForeignKey(nameof(ReactionID))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Reaction Reaction { get; set; } = null!;
}
