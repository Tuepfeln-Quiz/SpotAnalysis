namespace SpotAnalysis.Data.Models;

public class Observation {
    [Key]
    public int ObservationID { get; set; }

    [Required]
    public string Description { get; set; } = null!;


    public virtual ICollection<Reaction> Reactions { get; set; } = [];

}