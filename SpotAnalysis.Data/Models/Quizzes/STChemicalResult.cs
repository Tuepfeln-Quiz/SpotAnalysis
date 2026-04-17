namespace SpotAnalysis.Data.Models.Quizzes;

/// <summary>
/// For every Chemical in the STAvailableChemicals table the user has to choose a formula. This table stores the user's choice and whether it is correct or not.
/// </summary>

public class STChemicalResult {
    [Key]
    public int ChemicalResultID { get; set; }
    public int ResultID { get; set; }
    public int ChemicalID { get; set; }

    [Required]
    [StringLength(256)]
    public string ChosenFormula { get; set; } = null!;
    public bool IsCorrect { get; set; }
    
    [ForeignKey(nameof(ResultID))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public STResult Result { get; set; } = null!;

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Chemical Chemical { get; set; } = null!;
}