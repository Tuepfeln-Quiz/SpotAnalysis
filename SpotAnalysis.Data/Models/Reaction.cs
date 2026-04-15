namespace SpotAnalysis.Data.Models;

public class Reaction {
    [Key]
    public int ReactionID { get; set; }

    [Required]
    public int Chemical1ID { get; private set; } // set only in Constructor or with SetChemicals method

    public int Chemical2ID { get; private set; } // set only in Constructor or with SetChemicals method

    [Required]
    public string RelevantProduct { get; set; } = null!;

    [Required]
    public string Formula { get; set; } =  null!;

    public int ObservationID { get; set; }

    public string? ImagePath { get; set; }


    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Chemical Chemical1 { get; private set; } = null!;

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Chemical Chemical2 { get; private set; } = null!;

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Observation Observation { get; set; } = null!;

    public virtual ICollection<STLResult> STLResults { get; set; } = [];

    public virtual ICollection<STLAvailableReaction> STLAvailableReactions { get; set; } = [];

    public virtual ICollection<STLQuestion> STLQuestions { get; set; } = [];

    // construcors

    private Reaction() { }

    /// <summary>
    /// Constructor that ensures that Chemical1 always has the smaller ChemicalID than Chemical2.
    /// </summary>
    /// <param name="chem1"></param>
    /// <param name="chem2"></param>
    public Reaction(Chemical chem1, Chemical chem2) {
        if (chem1.ChemicalID <= chem2.ChemicalID) {
            Chemical1 = chem1;
            Chemical2 = chem2;
        } else {
            Chemical1 = chem2;
            Chemical2 = chem1;
        }
    }

    /// <summary>
    /// Update method that ensures that Chemical1 always has the smaller ChemicalID than Chemical2.
    /// </summary>
    /// <param name="chem1"></param>
    /// <param name="chem2"></param>
    public void SetChemicals(Chemical chem1, Chemical chem2) {
        if (chem1.ChemicalID <= chem2.ChemicalID) {
            Chemical1 = chem1;
            Chemical2 = chem2;
        } else {
            Chemical1 = chem2;
            Chemical2 = chem1;
        }
    }
}
