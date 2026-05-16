namespace SpotAnalysis.Data.Models.Quizzes;

/// <summary>
/// Contains the chemicals that need to be identified in the specified SpotTestQuestion.
/// One SpotTestQuestion may contain multiple chemicals. The result per given chemical will be tracked in the STAvailableChemicals table.
/// </summary>
[PrimaryKey(nameof(QuestionId), nameof(ChemicalId))]
public class StAvailableChemical
{
    public int QuestionId { get; set; }
    public int ChemicalId { get; set; }
    public required int Order { get; set; }

    [ForeignKey(nameof(QuestionId))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public StQuestion StQuestion { get; set; } = null!;

    [ForeignKey(nameof(ChemicalId))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Chemical Chemical { get; set; } = null!;
}
