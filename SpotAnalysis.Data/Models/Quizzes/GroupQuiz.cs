namespace Data.Models.Quizzes;

[PrimaryKey(nameof(GroupID), nameof(QuizID))]
public class GroupQuiz {
    public int GroupID { get; set; }
    public int QuizID { get; set; }

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Group Group { get; set; } = null!;

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Quiz Quiz { get; set; } = null!;
}
