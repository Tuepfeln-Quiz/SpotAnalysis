namespace SpotAnalysis.Data.Models.Quizzes;

[Index(nameof(Type))]
public class Question {
    [Key]
    public int QuestionID { get; set; }
    public QuestionType Type { get; set; }
    
    [Required]
    public string Description { get; set; } = null!;
    public Guid CreatedBy { get; set; }


    [ForeignKey(nameof(CreatedBy))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public User Creator { get; set; } = null!;

    public virtual ICollection<STLInput> STLInputs { get; set; } = [];
    public virtual ICollection<STAvailableChemical> STAvailableChemicals { get; set; } = [];
    public virtual ICollection<STAvailableMethod> STAvailableMehtods { get; set; } = [];
    public virtual ICollection<STLAvailableReaction> STAvailableReactions { get; set; } = [];
}
