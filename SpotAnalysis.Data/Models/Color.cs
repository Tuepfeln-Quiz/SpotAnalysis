namespace SpotAnalysis.Data.Models;

[Index(nameof(Name), IsUnique = true)]
public class Color
{
    [Key] public int ColorId { get; set; }

    [Required] [StringLength(128)] public string Name { get; set; } = null!;

    [Required] [StringLength(7)] public string HexValue { get; set; } = null!;


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

            if (max == r)
                h = ((g - b) / delta + (g < b ? 6 : 0)) * 60;
            else if (max == g)
                h = ((b - r) / delta + 2) * 60;
            else
                h = ((r - g) / delta + 4) * 60;
        }

        return new Hsl(
            Math.Round(h, 1),
            Math.Round(s * 100, 1),
            Math.Round(l * 100, 1));
    }

    /// <summary>
    /// see https://bottosson.github.io/posts/oklab/
    /// </summary>
    /// <returns></returns>
    public Oklab ToOklab()
    {
        var rgb = ToRgb();

        var lr = ToLinear(rgb.R / 255.0);
        var lg = ToLinear(rgb.G / 255.0);
        var lb = ToLinear(rgb.B / 255.0);

        var l = Math.Cbrt(0.4122214708 * lr + 0.5363325363 * lg + 0.0514459929 * lb);
        var m = Math.Cbrt(0.2119034982 * lr + 0.6806995451 * lg + 0.1073969566 * lb);
        var s = Math.Cbrt(0.0883024619 * lr + 0.2817188376 * lg + 0.6299787005 * lb);

        return new Oklab(
            Math.Round(0.2104542553 * l + 0.7936177850 * m - 0.0040720468 * s, 4),
            Math.Round(1.9779984951 * l - 2.4285922050 * m + 0.4505937099 * s, 4),
            Math.Round(0.0259040371 * l + 0.7827717662 * m - 0.8086757660 * s, 4));
    }

    /// <summary>
    /// see https://www.w3.org/TR/css-color-4/#color-conversion-code
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    private static double ToLinear(double c) =>
        c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
}