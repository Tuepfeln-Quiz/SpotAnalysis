namespace SpotAnalysis.Data.Models;

/// <summary>
/// Represents the observation (color) of the specified chemical when a specified method is applied to it.
/// </summary>
[PrimaryKey(nameof(ChemicalId), nameof(MethodId))]
public class MethodOutput
{
    public int ChemicalId { get; set; }
    public int MethodId { get; set; }

    public int ColorId { get; set; }


    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Chemical Chemical { get; set; } = null!;

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Method Method { get; set; } = null!;

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Color Color { get; set; } = null!;
}
