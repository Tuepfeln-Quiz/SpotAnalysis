namespace SpotAnalysis.Data.Models.Quizzes;

/// <summary>
/// Contains the methods that can be used in the specified SpotTestQuestion.
/// </summary>
[PrimaryKey(nameof(QuestionId), nameof(MethodId))]
public class StAvailableMethod
{
    public int QuestionId { get; set; }
    public int MethodId { get; set; }

    [ForeignKey(nameof(QuestionId))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public StQuestion StQuestion { get; set; } = null!;

    [ForeignKey(nameof(MethodId))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Method Method { get; set; } = null!;
}
