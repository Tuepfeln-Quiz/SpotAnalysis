namespace Data.Models.SpotTest;


[PrimaryKey(nameof(QuestionID), nameof(ChemicalID))] 
public class STAvailableChemical {
    public int QuestionID { get; set; }
    public int ChemicalID { get; set; }


    [DeleteBehavior(DeleteBehavior.Restrict)]
    public STQuestion Question { get; set; } = null!;

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Chemical Chemical { get; set; } = null!;
}
