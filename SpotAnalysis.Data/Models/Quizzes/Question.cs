namespace SpotAnalysis.Data.Models.Quizzes;

/// <summary>
/// A question that contains info about the QuestionType, a description. One question may be used in multiple Quizzes.
/// The nullable Reaction Property is only used for SpotTestLight questions.
/// </summary>

[Index(nameof(Type))]
public class Question {
    [Key]
    public int QuestionID { get; set; }

    /// <summary>
    /// not visible to users
    /// </summary>
    public string Title { get; set; } = null!;

    public QuestionType Type { get; set; }

    /// <summary>
    /// only for SpotTestLight questions
    /// </summary>
    public int? ReactionID { get; set; }

    [Required]
    public string Description { get; set; } = null!;
    public Guid CreatedBy { get; set; }


    [ForeignKey(nameof(CreatedBy))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public User Creator { get; set; } = null!;

    /// <summary>
    /// only for SpotTestLight questions
    /// </summary>
    [ForeignKey(nameof(ReactionID))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public virtual Reaction? STLInput { get; set; } = null!;
    public virtual ICollection<STAvailableChemical> STAvailableChemicals { get; set; } = [];
    public virtual ICollection<STAvailableMethod> STAvailableMethods { get; set; } = [];
    public virtual ICollection<STLAvailableReaction> STLAvailableReactions { get; set; } = [];
    public virtual ICollection<QuizQuestion> QuizQuestions { get; set; } = [];
}