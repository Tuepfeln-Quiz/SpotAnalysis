namespace SpotAnalysis.Web.Models;

public static class ColorHelper
{
    private static readonly Dictionary<string, string> Colors = new(StringComparer.OrdinalIgnoreCase)
    {
        ["violett"] = "#8B00FF",
        ["orange"] = "#FF8C00",
        ["rot"] = "#DC143C",
        ["grün"] = "#228B22",
        ["gelb"] = "#FFD700",
        ["blau"] = "#2563EB",
        ["gelbgrün"] = "#9ACD32",
        ["karminrot"] = "#DC143C",
        ["weiß"] = "#FFFFFF",
        ["keine"] = "#666666",
    };

    public static string GetCssColor(string? colorName)
    {
        if (string.IsNullOrEmpty(colorName))
            return "#666666";

        return Colors.TryGetValue(colorName, out var color) ? color : "#666666";
    }

    public static string GetBorderColor(string? colorName)
    {
        if (string.IsNullOrEmpty(colorName) || colorName.Equals("keine", StringComparison.OrdinalIgnoreCase))
            return "transparent";

        return Colors.TryGetValue(colorName, out var color) ? color : "transparent";
    }
}
