namespace SpotAnalysis.Data.Models.Quizzes;

public class Quiz {
    [Key]
    public int QuizID { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    public int QuizStatusID { get; set; }


    [DeleteBehavior(DeleteBehavior.Restrict)]
    public QuizStatus QuizStatus { get; set; } = null!;

    
    public virtual ICollection<Group> Groups { get; set; } = [];
    public virtual ICollection<Question> Questions { get; set; } = [];
    public virtual ICollection<QuizAttempt> QuizAttempts { get; set; } = [];
}
