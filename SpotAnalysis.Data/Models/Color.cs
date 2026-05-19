namespace SpotAnalysis.Data.Models;

[Index(nameof(Name), IsUnique = true)]
public class Color
{
    [Key]
    public int ColorId { get; set; }

    [StringLength(128)]
    public required string Name { get; set; } = null!;

    [StringLength(7)]
    public required string HexValue { get; set; } = null!;

    public bool IsColorless { get; set; }

    public virtual ICollection<Chemical> Chemicals { get; set; } = [];
    public virtual ICollection<MethodOutput> MethodOutputs { get; set; } = [];


    public Rgb ToRgb()
    {
        var hex = HexValue.TrimStart('#');
        return new Rgb(
            Convert.ToByte(hex[..2], 16),
            Convert.ToByte(hex[2..4], 16),
            Convert.ToByte(hex[4..6], 16));
    }

    /// <summary>
    /// see https://www.w3.org/TR/css-color-4/#hsl-to-rgb
    /// </summary>
    /// <returns></returns>
    public Hsl ToHsl()
    {
        var rgb = ToRgb();
        var r = rgb.R / 255.0;
        var g = rgb.G / 255.0;
        var b = rgb.B / 255.0;

        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        var delta = max - min;

        var l = (max + min) / 2.0;
        double h = 0;
        double s = 0;

        if (delta > 0)
        {
            s = l > 0.5 ? delta / (2.0 - max - min) : delta / (max + min);

            var maxChannel = Math.Max(rgb.R, Math.Max(rgb.G, rgb.B));
            if (maxChannel == rgb.R)
                h = ((g - b) / delta + (g < b ? 6 : 0)) * 60;
            else if (maxChannel == rgb.G)
                h = ((b - r) / delta + 2) * 60;
            else
                h = ((r - g) / delta + 4) * 60;
        }

        return new Hsl(
            Math.Round(h, 1),
            Math.Round(s * 100, 1),
            Math.Round(l * 100, 1));
    }
}
