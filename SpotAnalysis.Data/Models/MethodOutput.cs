namespace Models;

[PrimaryKey(nameof(ChemicalID), nameof(MethodID))]
public class MethodOutput {
    public int ChemicalID { get; set; }
    public int MethodID { get; set; }

    [Required]
    public string Color { get; set; } = null!;


    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Chemical Chemical { get; set; } = null!;

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Method Method { get; set; } = null!;
}