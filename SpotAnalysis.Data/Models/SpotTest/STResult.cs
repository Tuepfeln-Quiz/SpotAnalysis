using System.ComponentModel.DataAnnotations.Schema;

namespace Models.SpotTest;

public class STResult {
    [Key]
    public int ResultID { get; set; }
    public int AttemptID { get; set; }
    public int QuestionID { get; set; }
    public bool IsCorrect { get; set; }


    [ForeignKey(nameof(AttemptID))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public QuizAttempt Attempt { get; set; } = null!;

    [ForeignKey(nameof(QuestionID))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public STQuestion Question { get; set; } = null!;

    public virtual ICollection<STChemicalResult> ChemicalResults { get; set; } = [];
    public virtual ICollection<STLog> STLogs { get; set; } = [];
}