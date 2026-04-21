using System.Diagnostics.CodeAnalysis;

namespace SpotAnalysis.Data.Models.Quizzes;

/// <summary>
/// A question that contains info about the QuestionType, a description. One question may be used in multiple Quizzes.
/// The nullable Reaction Property is only used for SpotTestLight questions.
/// </summary>

[Index(nameof(Type))]
public class Question
{
    [Key]
    public int QuestionID { get; set; }

    /// <summary>
    /// not visible to users
    /// </summary>
    [StringLength(256)]
    public required string Title { get; set; } = null!;

    public QuestionType Type { get; set; }

    [Required]
    [StringLength(1024)]
    public string Description { get; set; } = null!;
    public Guid? CreatedBy { get; set; }


    [ForeignKey(nameof(CreatedBy))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public User Creator { get; set; } = null!;


    [DeleteBehavior(DeleteBehavior.Restrict)]
    public virtual STLQuestion? STLQuestion { get; set; } = null!;

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public virtual STQuestion? STQuestion { get; set; } = null!;
    public virtual ICollection<QuizQuestion> QuizQuestions { get; set; } = [];
}