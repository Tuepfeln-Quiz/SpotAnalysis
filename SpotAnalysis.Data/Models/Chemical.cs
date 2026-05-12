namespace SpotAnalysis.Data.Models;

/// <summary>
/// Contains all chemicals that are used in the reactions. Chemicals can be used as educts or additives in a reaction. They can also be used with methods to produce an observation (MethodOutput).
/// </summary>
[Index(nameof(Type))]
public class Chemical
{
    [Key] public int ChemicalId { get; set; }

    public ChemicalType Type { get; set; }

    [StringLength(256)] public required string Name { get; set; } = null!;

    [StringLength(256)] public required string Formula { get; set; } = null!;

    public int ColorId { get; set; }

    [StringLength(256)] public string? ImagePath { get; set; }


    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Color Color { get; set; } = null!;

    public virtual ICollection<MethodOutput> MethodOutputs { get; set; } = [];

    [InverseProperty(nameof(Reaction.Chemical1))]
    public virtual ICollection<Reaction> Chemical1Reactions { get; set; } = [];

    [InverseProperty(nameof(Reaction.Chemical2))]
    public virtual ICollection<Reaction> Chemical2Reactions { get; set; } = [];

    public virtual ICollection<StAvailableChemical> StAvailableChemicals { get; set; } = [];

    public virtual ICollection<StResult> StResults { get; set; } = [];
    public virtual ICollection<StChemicalResult> StChemicalResults { get; set; } = [];

    public virtual ICollection<StlQuestion> StlQuestions { get; set; } = [];
}
