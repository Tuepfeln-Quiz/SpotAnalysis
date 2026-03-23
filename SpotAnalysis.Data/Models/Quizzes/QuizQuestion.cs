namespace SpotAnalysis.Data.Models.Quizzes;

[PrimaryKey(nameof(QuizID), nameof(QuestionID))]
public class QuizQuestion {
    public int QuizID { get; set; }
    public int QuestionID { get; set; }
    public int OrderID { get; set; }


    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Quiz Quiz { get; set; } = null!;
    
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Question Question { get; set; } = null!;
}
