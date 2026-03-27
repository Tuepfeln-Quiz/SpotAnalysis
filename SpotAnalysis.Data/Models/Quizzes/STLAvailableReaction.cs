namespace SpotAnalysis.Data.Models.Quizzes;


[PrimaryKey(nameof(QuestionID), nameof(ReactionID))]
public class STLAvailableReaction {
    public int QuestionID { get; set; }
    public int ReactionID { get; set; }


    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Question Question { get; set; } = null!;

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Reaction Reaction { get; set; } = null!;
}
