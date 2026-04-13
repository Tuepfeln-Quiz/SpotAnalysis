namespace SpotAnalysis.Data.Models.Quizzes;


/// <summary>
/// Contains the chemicals that need to be identified in the specified SpotTestQuestion.
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
