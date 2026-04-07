namespace SpotAnalysis.Data.Models.Quizzes;

[Table("QuizAttempts")]
[PrimaryKey(nameof(AttemptID))]
public class QuizAttempt {
    public int AttemptID { get; set; }
    public Guid UserID { get; set; }
    
    public int QuizID { get; set; }
    public DateTime Started { get; set; }
    public DateTime Completed { get; set; }

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public User User { get; set; } = null!;

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Quiz Quiz { get; set; } = null!;
    public virtual ICollection<STLResult> STLResults { get; set; } = [];
    public virtual ICollection<STResult> STResults { get; set; } = [];
}
