namespace SpotAnalysis.Web.Models;

public static class FlameColorHelper
{
    private static readonly Dictionary<string, string> FlameColors = new(StringComparer.OrdinalIgnoreCase)
    {
        ["violett"] = "#8B00FF",
        ["orange"] = "#FF8C00",
        ["rot"] = "#DC143C",
        ["grün"] = "#228B22",
        ["gelb"] = "#FFD700",
        ["blau"] = "#00CED1",
        ["gelbgrün"] = "#9ACD32",
        ["karminrot"] = "#DC143C",
        ["keine"] = "#999999",
    };

    public static string GetCssColor(string? flameName)
    {
        if (string.IsNullOrEmpty(flameName))
            return "#999999";

        return FlameColors.TryGetValue(flameName, out var color) ? color : "#999999";
    }
}
