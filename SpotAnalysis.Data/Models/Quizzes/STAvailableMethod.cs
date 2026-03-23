namespace SpotAnalysis.Data.Models.Quizzes;

[PrimaryKey(nameof(QuestionID), nameof(MethodID))]
public class STAvailableMethod {
    public int QuestionID { get; set; }
    public int MethodID { get; set; }


    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Question Question { get; set; } = null!;

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Method Method { get; set; } = null!;
}
