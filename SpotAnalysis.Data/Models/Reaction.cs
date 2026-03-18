using Data.Models.SpotTestLight;

namespace Data.Models;

public class Reaction {
    [Key]
    public int ReactionID { get; set; }

    [Required]
    public int Chemical1ID { get; set; }

    public int Chemical2ID { get; set; }

    [Required]
    public string RelevantProduct { get; set; } = null!;

    [Required]
    public string Formula { get; set; } =  null!;

    public int ObservationID { get; set; }

    public string? ImagePath { get; set; }


    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Chemical Chemical1 { get; set; } = null!;

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Chemical Chemical2 { get; set; } = null!;

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Observation Observation { get; set; } = null!;
    public virtual ICollection<STLResult> STLResults { get; set; } = [];

    public virtual ICollection<STLAvailableReaction> STLAvailableReactions { get; set; } = [];
}
