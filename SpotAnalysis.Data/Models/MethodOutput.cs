namespace SpotAnalysis.Data.Models;

/// <summary>
/// Represents the observation (color) of the specified chemical when a specified method is applied to it.
/// </summary>

[PrimaryKey(nameof(ChemicalID), nameof(MethodID))]
public class MethodOutput
{
    public int ChemicalID { get; set; }
    public int MethodID { get; set; }

    [Required]
    [StringLength(128)]
    public string Color { get; set; } = null!;


    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Chemical Chemical { get; set; } = null!;

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Method Method { get; set; } = null!;
}