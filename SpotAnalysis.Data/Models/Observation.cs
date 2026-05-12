namespace SpotAnalysis.Data.Models;

/// <summary>
/// Lookup table for every available Observation in a Reaction.
/// </summary>
public class Observation
{
    [Key] public int ObservationId { get; set; }

    [StringLength(512)] public required string Description { get; set; } = null!;


    public virtual ICollection<Reaction> Reactions { get; set; } = [];
}
