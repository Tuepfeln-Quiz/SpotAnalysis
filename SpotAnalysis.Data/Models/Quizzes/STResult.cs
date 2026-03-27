namespace SpotAnalysis.Data.Models.Quizzes;

public class STResult {
    [Key]
    public int ResultID { get; set; }
    public int AttemptID { get; set; }
    public int QuestionID { get; set; }


    [ForeignKey(nameof(AttemptID))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public QuizAttempt Attempt { get; set; } = null!;

    [ForeignKey(nameof(QuestionID))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Question Question { get; set; } = null!;

    public virtual ICollection<STChemicalResult> ChemicalResults { get; set; } = [];
    public virtual ICollection<STLog> STLogs { get; set; } = [];
}