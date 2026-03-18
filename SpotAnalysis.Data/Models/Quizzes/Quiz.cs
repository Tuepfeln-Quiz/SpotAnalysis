namespace Data.Models.Quizzes;

public class Quiz {
    [Key]
    public int QuizID { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    public int QuizTypeID { get; set; }
    public int QuizStatusID { get; set; }


    [DeleteBehavior(DeleteBehavior.Restrict)]
    public QuizStatus QuizStatus { get; set; } = null!;

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public QuizType QuizType { get; set; } = null!;
    
    public virtual ICollection<GroupQuiz> GroupQuizzes { get; set; } = [];
    public virtual ICollection<STQuestion> STQuestions { get; set; } = [];
    public virtual ICollection<STLQuestion> STLQuestions { get; set; } = [];
    public virtual ICollection<QuizAttempt> QuizAttempts { get; set; } = [];
}
