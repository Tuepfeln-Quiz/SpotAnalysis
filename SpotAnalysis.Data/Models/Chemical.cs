namespace SpotAnalysis.Data.Models;


[Index(nameof(Type))]
public class Chemical {
    [Key]
    public int ChemicalID { get; set; }
    public ChemicalType Type { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string Formula { get; set; } = null!;

    [Required]
    public string Color { get; set; } = null!;

    public string? ImagePath { get; set; }


    public virtual ICollection<MethodOutput> MethodOutputs { get; set; } = [];

    [InverseProperty(nameof(Reaction.Chemical1))]
    public virtual ICollection<Reaction> Chemical1Reactions { get; set; } = [];

    [InverseProperty(nameof(Reaction.Chemical2))]
    public virtual ICollection<Reaction> Chemical2Reactions { get; set; } = [];

    public virtual ICollection<STAvailableChemical> STAvailableChemicals { get; set; } = [];

    public virtual ICollection<STResult> STResults { get; set; } = [];
    public virtual ICollection<STLResult> STLResults { get; set; } = [];
    public virtual ICollection<STChemicalResult> STChemicalResults { get; set; } = [];
}
