namespace SpotAnalysis.Data.Models.Quizzes; 

public class Question {
    [Key]
    public int QuestionID { get; set; }
    public int QuestionTypeID { get; set; }
    
    [Required]
    public string Description { get; set; } = null!;


    [DeleteBehavior(DeleteBehavior.Restrict)]
    public QuestionType QuestionType { get; set; } = null!;

    public virtual ICollection<STLInput> STLInputs { get; set; } = [];
    public virtual ICollection<STAvailableChemical> STAvailableChemicals { get; set; } = [];
    public virtual ICollection<STAvailableMethod> STAvailableMehtods { get; set; } = [];
    public virtual ICollection<STLAvailableReaction> STAvailableReactions { get; set; } = [];
}
