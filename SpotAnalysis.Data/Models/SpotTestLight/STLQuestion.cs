namespace Data.Models.SpotTestLight;

public class STLQuestion {
    [Key]
    public int QuestionID { get; set; }
    public int QuizID { get; set; }
    public int ChemicalID { get; set; }
    public int ObservationID { get; set; }

    [Required]
    public string Description { get; set; } = null!;


    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Quiz Quiz { get; set; } = null!;

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Chemical Chemical { get; set; } = null!;

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Observation Observation { get; set; } = null!;
    public virtual ICollection<STLResult> STLResults { get; set; } = [];
    public virtual ICollection<STLAvailableReaction> AvailableReactions { get; set; } = [];
}
