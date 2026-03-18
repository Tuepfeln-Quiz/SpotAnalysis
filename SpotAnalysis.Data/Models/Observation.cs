using DataAccessLayer.Models.SpotTestLight;

namespace DataAccessLayer.Models; 
public class Observation {
    [Key]
    public int ObservationID { get; set; }

    [Required]
    public string Description { get; set; } = null!;


    public virtual ICollection<Reaction> Reactions { get; set; } = [];
    public virtual ICollection<STLQuestion> STLQuestions { get; set; } = [];
}