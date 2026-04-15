namespace SpotAnalysis.Data.Models.Quizzes;

/// <summary>
/// Contains the input for a SpotTestLight question, which is the chemical of type educt shown to the quiztaker and the expected reaction. The reaction contains the Observation (also visible to quiztaker) as well as the second chemical the student needs to identify.
/// </summary>

public class STLQuestion {
    [Key]
    [ForeignKey(nameof(QuestionID))]
    public int QuestionID { get; set; }
    public int ReactionID { get; set; }
    public int ShownEductID { get; set; }


    [DeleteBehavior(DeleteBehavior.Restrict)]
    public virtual Question Question { get; set; } = null!;

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public virtual Reaction Reaction { get; set; } = null!;

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public virtual Chemical ShownEduct { get; set; } = null!;

    public virtual ICollection<STLAvailableReaction> AvailableReactions { get; set; } = [];
    public virtual ICollection<STAvailableChemical> AvailableChemicals { get; set; } = [];
}
