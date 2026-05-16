namespace SpotAnalysis.Data.Models;

public class Reaction
{
    // construcors

    private Reaction() { }

    /// <summary>
    /// Constructor that ensures that Chemical1 always has the smaller ChemicalID than Chemical2.
    /// </summary>
    /// <param name="chem1"></param>
    /// <param name="chem2"></param>
    public Reaction(Chemical chem1, Chemical chem2)
    {
        if (chem1.ChemicalId <= chem2.ChemicalId)
        {
            Chemical1 = chem1;
            Chemical2 = chem2;
        }
        else
        {
            Chemical1 = chem2;
            Chemical2 = chem1;
        }
    }

    [Key] public int ReactionId { get; set; }

    [Required] public int Chemical1Id { get; private set; } // set only in Constructor or with SetChemicals method

    public int Chemical2Id { get; private set; } // set only in Constructor or with SetChemicals method

    [StringLength(256)] public required string RelevantProduct { get; set; } = null!;

    [StringLength(256)] public required string Formula { get; set; } = null!;

    public int ObservationId { get; set; }

    [StringLength(256)] public string? ImagePath { get; set; }


    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Chemical Chemical1 { get; private set; } = null!;

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Chemical Chemical2 { get; private set; } = null!;

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Observation Observation { get; set; } = null!;

    public virtual ICollection<StlResult> StlResults { get; set; } = [];

    public virtual ICollection<StlAvailableReaction> StlAvailableReactions { get; set; } = [];

    public virtual ICollection<StlQuestion> StlQuestions { get; set; } = [];

    /// <summary>
    /// Update method that ensures that Chemical1 always has the smaller ChemicalID than Chemical2.
    /// </summary>
    /// <param name="chem1"></param>
    /// <param name="chem2"></param>
    public void SetChemicals(Chemical chem1, Chemical chem2)
    {
        if (chem1.ChemicalId <= chem2.ChemicalId)
        {
            Chemical1 = chem1;
            Chemical2 = chem2;
        }
        else
        {
            Chemical1 = chem2;
            Chemical2 = chem1;
        }
    }
}
