namespace SpotAnalysis.Data.Models.Quizzes; 
public class STQuestion {
    [Key]
    [ForeignKey(nameof(QuestionID))]
    public int QuestionID { get; set; }

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public virtual Question Question { get; set; } = null!;

    public virtual ICollection<STAvailableChemical> STAvailableChemicals { get; set; } = [];
    public virtual ICollection<STAvailableMethod> STAvailableMethods { get; set; } = [];
    public virtual ICollection<STLAvailableReaction> STLAvailableReactions { get; set; } = [];
}
