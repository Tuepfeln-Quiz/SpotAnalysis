namespace SpotAnalysis.Data.Models.Quizzes;

/// <summary>
/// Contains the methods that can be used in the specified SpotTestQuestion.
/// </summary>
[PrimaryKey(nameof(QuestionId), nameof(Method))]
public class StAvailableMethod
{
    public int QuestionId { get; set; }

    [ForeignKey(nameof(QuestionId))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public StQuestion StQuestion { get; set; } = null!;

    public Method Method { get; set; }
}
