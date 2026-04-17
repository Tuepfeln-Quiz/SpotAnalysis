namespace SpotAnalysis.Data.Models.Quizzes;

public class Quiz {
    [Key]
    public int QuizID { get; set; }

    [Required]
    [StringLength(128)]
    public string Name { get; set; } = null!;
    public Guid? CreatedBy { get; set; }


    [ForeignKey(nameof(CreatedBy))]
    [DeleteBehavior(DeleteBehavior.SetNull)]
    public User? Creator { get; set; }

    public virtual ICollection<Group> Groups { get; set; } = [];
    public virtual ICollection<Question> Questions { get; set; } = [];
    public virtual ICollection<QuizAttempt> Attempts { get; set; } = [];

    public virtual ICollection<QuizQuestion> QuizQuestions { get; set; } = [];
}
