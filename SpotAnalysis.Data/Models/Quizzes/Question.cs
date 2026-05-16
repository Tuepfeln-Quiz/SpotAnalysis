namespace SpotAnalysis.Data.Models.Quizzes;

/// <summary>
/// A question that contains info about the QuestionType, a description. One question may be used in multiple Quizzes.
/// The nullable Reaction Property is only used for SpotTestLight questions.
/// </summary>
[Index(nameof(Type))]
public class Question
{
    [Key] public int QuestionId { get; set; }

    /// <summary>
    /// not visible to users
    /// </summary>
    [StringLength(256)]
    public required string Title { get; set; } = null!;

    public QuestionType Type { get; set; }

    [StringLength(1024)] public required string Description { get; set; } = null!;

    public Guid? CreatedBy { get; set; }


    [ForeignKey(nameof(CreatedBy))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public virtual User Creator { get; set; } = null!;


    [DeleteBehavior(DeleteBehavior.Restrict)]
    public virtual StlQuestion? StlQuestion { get; set; } = null!;

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public virtual StQuestion? StQuestion { get; set; } = null!;

    public virtual ICollection<QuizQuestion> QuizQuestions { get; set; } = [];
}
