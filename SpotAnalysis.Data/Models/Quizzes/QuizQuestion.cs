namespace SpotAnalysis.Data.Models.Quizzes;

/// <summary>
///  Represents the association between a Quiz and a Question, allowing for a many-to-many relationship between them.
/// </summary>

[PrimaryKey(nameof(QuizID), nameof(QuestionID))]
public class QuizQuestion
{
    public int QuizID { get; set; }
    public int QuestionID { get; set; }
    public int Order { get; set; }


    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Quiz Quiz { get; set; } = null!;

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Question Question { get; set; } = null!;
}
