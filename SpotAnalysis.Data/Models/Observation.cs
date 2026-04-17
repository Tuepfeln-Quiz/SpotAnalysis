namespace SpotAnalysis.Data.Models;

/// <summary>
/// Lookup table for every available Observation in a Reaction.
/// </summary>

public class Observation {
    [Key]
    public int ObservationID { get; set; }

    [Required]
    [StringLength(512)]
    public string Description { get; set; } = null!;


    public virtual ICollection<Reaction> Reactions { get; set; } = [];

}