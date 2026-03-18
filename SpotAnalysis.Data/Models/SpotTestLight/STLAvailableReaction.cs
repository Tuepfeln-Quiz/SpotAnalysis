namespace DataAccessLayer.Models.SpotTestLight;


[PrimaryKey(nameof(QuestionID), nameof(ReactionID))]
public class STLAvailableReaction {
    public int QuestionID { get; set; }
    public int ReactionID { get; set; }


    [DeleteBehavior(DeleteBehavior.Restrict)]
    public STLQuestion STLQuestion { get; set; } = null!;

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Reaction Reaction { get; set; } = null!;
}
