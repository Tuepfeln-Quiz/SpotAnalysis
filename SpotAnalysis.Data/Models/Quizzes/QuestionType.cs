namespace SpotAnalysis.Data.Models.Quizzes;

public class QuestionType {
    [Key]
    public int QuestionTypeID { get; set; }

    [Required]
    public string Name { get; set; } = null!;


    public virtual ICollection<Question> Questions { get; set; } = [];
}