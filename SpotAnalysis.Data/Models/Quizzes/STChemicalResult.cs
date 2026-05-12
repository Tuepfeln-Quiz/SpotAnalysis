namespace SpotAnalysis.Data.Models.Quizzes;

/// <summary>
/// For every Chemical in the STAvailableChemicals table the user has to choose a formula. This table stores the user's choice and whether it is correct or not.
/// </summary>
public class StChemicalResult
{
    [Key] public int ChemicalResultId { get; set; }

    public int ResultId { get; set; }
    public int ChemicalId { get; set; }

    [StringLength(256)] public required string ChosenFormula { get; set; } = null!;

    public bool IsCorrect { get; set; }

    [ForeignKey(nameof(ResultId))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public StResult Result { get; set; } = null!;

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Chemical Chemical { get; set; } = null!;
}
