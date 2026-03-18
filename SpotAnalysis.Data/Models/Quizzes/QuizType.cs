namespace DataAccessLayer.Models.Quizzes;

public class QuizType {
    [Key]
    public int QuizTypeID { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    public virtual ICollection<Quiz> Quizzes { get; set; } = [];
}