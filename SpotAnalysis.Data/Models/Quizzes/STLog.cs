namespace SpotAnalysis.Data.Models.Quizzes;

public class STLog {
    [Key]
    public int LogID { get; set; }
    public int ResultID { get; set; }
    public int Chemical1ID { get; set; }
    public int Chemical2ID { get; set; }

    
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public STResult Result { get; set; } = null!;

    [ForeignKey(nameof(Chemical1ID))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Chemical Chemical1 { get; set; } = null!;

    [ForeignKey(nameof(Chemical2ID))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Chemical Chemical2 { get; set; } = null!;
}
