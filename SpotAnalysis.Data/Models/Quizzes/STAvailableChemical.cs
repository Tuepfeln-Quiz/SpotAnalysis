namespace SpotAnalysis.Data.Models.Quizzes;


/// <summary>
/// Contains the chemicals that need to be identified in the specified SpotTestQuestion.
/// One SpotTestQuestion may contain multiple chemicals. The result per given chemical will be tracked in the STAvailableChemicals table.
/// </summary>

[PrimaryKey(nameof(QuestionID), nameof(ChemicalID))] 
public class STAvailableChemical {
    public int QuestionID { get; set; }
    public int ChemicalID { get; set; }
    public required int Order { get; set; }

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Question Question { get; set; } = null!;

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Chemical Chemical { get; set; } = null!;
}
