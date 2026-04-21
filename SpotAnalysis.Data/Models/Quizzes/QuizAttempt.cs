namespace SpotAnalysis.Data.Models.Quizzes;

/// <summary>
/// Every time a user starts a quiz, a new QuizAttempt is created.
/// Tracks the start and completion times, as well as the associated user and quiz.
/// </summary>

[Table("QuizAttempts")]
[PrimaryKey(nameof(AttemptID))]
public class QuizAttempt
{
    public int AttemptID { get; set; }
    public Guid UserID { get; set; }

    public int QuizID { get; set; }
    public DateTime Started { get; set; }
    public DateTime? Completed { get; set; }

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public User User { get; set; } = null!;

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Quiz Quiz { get; set; } = null!;
    public virtual ICollection<STLResult> STLResults { get; set; } = [];
    public virtual ICollection<STResult> STResults { get; set; } = [];
}
