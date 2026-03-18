using DataAccessLayer.Models.SpotTestLight;
using DataAccessLayer.Models.SpotTest;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer.Models;


[Index(nameof(ChemicalTypeID))]
public class Chemical {
    [Key]
    public int ChemicalID { get; set; }

    public int ChemicalTypeID { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string Formula { get; set; } = null!;

    [Required]
    public string Color { get; set; } = null!;

    public string? ImagePath { get; set; }


    [DeleteBehavior(DeleteBehavior.Restrict)]
    public ChemicalType ChemicalType { get; set; } = null!;

    public virtual ICollection<MethodOutput> MehtodOutputs { get; set; } = [];

    [InverseProperty(nameof(Reaction.Chemical1))]
    public virtual ICollection<Reaction> Chemical1Reactions { get; set; } = [];

    [InverseProperty(nameof(Reaction.Chemical2))]
    public virtual ICollection<Reaction> Chemical2Reactions { get; set; } = [];

    public virtual ICollection<STAvailableChemical> STAvailableChemicals { get; set; } = [];
    public virtual ICollection<STLQuestion> STLQuestions {  get; set; } = [];
    public virtual ICollection<STQuestion> STQuestions { get; set; } = [];

    public virtual ICollection<STResult> STResults { get; set; } = [];
    public virtual ICollection<STLResult> STLResults { get; set; } = [];
}
