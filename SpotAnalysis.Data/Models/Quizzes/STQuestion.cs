namespace SpotAnalysis.Data.Models.Quizzes;

public class StQuestion
{
    [Key] [ForeignKey(nameof(QuestionId))] public int QuestionId { get; set; }

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public virtual Question Question { get; set; } = null!;

    public virtual ICollection<StAvailableChemical> AvailableChemicals { get; set; } = [];
    public virtual ICollection<StAvailableMethod> AvailableMethods { get; set; } = [];
}
