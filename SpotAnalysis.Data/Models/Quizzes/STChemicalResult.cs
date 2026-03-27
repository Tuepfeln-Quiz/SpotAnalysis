namespace SpotAnalysis.Data.Models.Quizzes;

public class STChemicalResult {
    [Key]
    public int ChemicalResultID { get; set; }
    public int ResultID { get; set; }
    public int ChemicalID { get; set; }

    [Required]
    public string ChosenFormula { get; set; } = null!;

    public bool IsCorrect { get; set; }


    [ForeignKey(nameof(ResultID))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public STResult Result { get; set; } = null!;

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Chemical Chemical { get; set; } = null!;
}