namespace SpotAnalysis.Data.Models.Quizzes;

[Index(nameof(Type))]
public class Question {
    [Key]
    public int QuestionID { get; set; }
    public QuestionType Type { get; set; }

    public int ReactionID { get; set; }

    [Required]
    public string Description { get; set; } = null!;
    public Guid CreatedBy { get; set; }


    [ForeignKey(nameof(CreatedBy))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public User Creator { get; set; } = null!;

    [ForeignKey(nameof(ReactionID))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public virtual Reaction? STLInput { get; set; } = null!;
    public virtual ICollection<STAvailableChemical> STAvailableChemicals { get; set; } = [];
    public virtual ICollection<STAvailableMethod> STAvailableMethods { get; set; } = [];
    public virtual ICollection<STLAvailableReaction> STLAvailableReactions { get; set; } = [];
    public virtual ICollection<QuizQuestion> QuizQuestions { get; set; } = [];
}