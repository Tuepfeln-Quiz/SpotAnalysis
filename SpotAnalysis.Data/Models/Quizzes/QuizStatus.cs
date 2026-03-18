namespace DataAccessLayer.Models.Quizzes; 
public class QuizStatus {
    [Key]
    public int QuizStatusID { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    public virtual ICollection<Quiz> Quizzes { get; set; } = [];
}
