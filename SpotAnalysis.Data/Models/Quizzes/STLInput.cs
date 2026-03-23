namespace SpotAnalysis.Data.Models.Quizzes; 

public class STLInput {
    [Key] [ForeignKey(nameof(Question))]
    public int QuestionID { get; set; }
    public int ObservationID { get; set; }
    public int ChemicalID { get; set; }


    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Question Question { get; set; } = null!;

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Observation Observation { get; set; } = null!;

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Chemical Chemical { get; set; } = null!;
}