namespace Data.Models.SpotTest; 
public class STQuestion {
    [Key]
    public int QuestionID { get; set; }
    public int QuizID { get; set; }

    [Required]
    public string Description { get; set; } = null!;


    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Quiz Quiz { get; set; } = null!;

    public virtual ICollection<STAvailableChemical> AvailableChemicals { get; set; } = [];
    public virtual ICollection<STAvailableMethod> AvailableMethods { get; set; } = [];
    public virtual ICollection<STResult> Results { get; set; } = [];
}
