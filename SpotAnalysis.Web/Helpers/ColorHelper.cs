namespace SpotAnalysis.Web.Helpers;

public static class ColorHelper
{
    // Fallback-Farbe für unbekannte Eingaben (neutrales Grau).
    private const string FallbackColor = "#666666";

    // Spezielle Schlüssel, die in Borders als "transparent" erscheinen sollen
    // (inkrementell erkennbar: farblose oder nicht vorhandene Beobachtungen).
    private static readonly HashSet<string> TransparentKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "keine", "nichts", "farblos", "durchsichtig", "klar"
    };

    // Chemie-Kontext: Eigenfarben von Feststoffen und Lösungen, Flammenfärbungen,
    // pH-Indikator-Farben und häufige Reaktionsbeobachtungen im Tüpfel-Test.
    // Case-insensitive Lookup; Keys werden zusätzlich getrimmt.
    private static readonly Dictionary<string, string> Colors = new(StringComparer.OrdinalIgnoreCase)
    {
        // ── Neutrale Zustände / "keine Farbe" ─────────────────────────────
        ["keine"]          = FallbackColor,
        ["nichts"]         = FallbackColor,
        ["farblos"]        = "#DDDDDD",
        ["durchsichtig"]   = "#DDDDDD",
        ["klar"]           = "#DDDDDD",
        ["milchig"]        = "#F5F5DC",
        ["trüb"]           = "#C9D6DE",
        ["trueb"]          = "#C9D6DE",

        // ── Weiß / Schwarz / Grau ─────────────────────────────────────────
        ["weiß"]           = "#FFFFFF",
        ["weiss"]          = "#FFFFFF",
        ["schwarz"]        = "#000000",
        ["grau"]           = "#808080",
        ["hellgrau"]       = "#D3D3D3",
        ["dunkelgrau"]     = "#505050",

        // ── Rot-Familie (Flammenfärbung: Li, Sr, Ca; Fe(III)-Lösungen) ────
        ["rot"]            = "#DC143C",
        ["hellrot"]        = "#FF6B6B",
        ["dunkelrot"]      = "#8B0000",
        ["karminrot"]      = "#960018",
        ["ziegelrot"]      = "#B22222",
        ["weinrot"]        = "#722F37",
        ["blutrot"]        = "#8A0303",
        ["kirschrot"]      = "#990033",
        ["rostrot"]        = "#B7410E",
        ["purpurrot"]      = "#800080",
        ["magenta"]        = "#FF00FF",

        // ── Rosa / Pink (Phenolphthalein, Permanganat verd.) ──────────────
        ["rosa"]           = "#FFB6C1",
        ["hellrosa"]       = "#FFD1DC",
        ["dunkelrosa"]     = "#FF1493",
        ["pink"]           = "#FF69B4",

        // ── Orange-Familie (Fe(III), Methylorange) ────────────────────────
        ["orange"]         = "#FF8C00",
        ["hellorange"]     = "#FFB347",
        ["dunkelorange"]   = "#CC6600",
        ["orangerot"]      = "#FF4500",
        ["orangegelb"]     = "#FFA500",

        // ── Gelb-Familie (Flammenfärbung: Na; Blei(II)-Salze, S) ──────────
        ["gelb"]           = "#FFD700",
        ["hellgelb"]       = "#FFFACD",
        ["dunkelgelb"]     = "#B8860B",
        ["goldgelb"]       = "#DAA520",
        ["zitronengelb"]   = "#FFF44F",
        ["gelbgrün"]       = "#9ACD32",
        ["gelbbraun"]      = "#D2B48C",

        // ── Grün-Familie (Flammenfärbung: Ba, Cu, B; Ni(II), Cr(III)) ─────
        ["grün"]           = "#228B22",
        ["gruen"]          = "#228B22",
        ["hellgrün"]       = "#90EE90",
        ["dunkelgrün"]     = "#006400",
        ["blaugrün"]       = "#008B8B",
        ["smaragdgrün"]    = "#50C878",
        ["olivgrün"]       = "#6B8E23",
        ["olive"]          = "#808000",
        ["grasgrün"]       = "#4F7942",
        ["giftgrün"]       = "#4CBB17",
        ["türkisgrün"]     = "#20B2AA",

        // ── Blau-Familie (Flammenfärbung: Cu-Halogenide; Cu(II)-Lösung) ───
        ["blau"]           = "#2563EB",
        ["hellblau"]       = "#87CEEB",
        ["dunkelblau"]     = "#00008B",
        ["fahlblau"]       = "#6497B1",
        ["königsblau"]     = "#4169E1",
        ["marineblau"]     = "#1F3A93",
        ["himmelblau"]     = "#87CEEB",
        ["stahlblau"]      = "#4682B4",
        ["türkisblau"]     = "#00CED1",
        ["türkis"]         = "#40E0D0",

        // ── Violett / Lila (Flammenfärbung: K; Permanganat, Iod-Stärke) ───
        ["violett"]        = "#8B00FF",
        ["hellviolett"]    = "#B366FF",
        ["dunkelviolett"]  = "#5C0099",
        ["blauviolett"]    = "#8A2BE2",
        ["rotviolett"]     = "#B01E7A",
        ["lila"]           = "#A020F0",
        ["fliederfarben"]  = "#C8A2C8",

        // ── Braun-Familie (Iod, Fe(III)-Verbindungen, Rost) ───────────────
        ["braun"]          = "#8B4513",
        ["hellbraun"]      = "#CD853F",
        ["dunkelbraun"]    = "#5D2E0A",
        ["schokobraun"]    = "#3E2218",
        ["rotbraun"]       = "#8B3A1F",
        ["rostbraun"]      = "#B7410E",
        ["kupferbraun"]    = "#994D2A",
        ["beige"]          = "#F5F5DC",

        // ── Metallisch (Alkalimetalle, Edelmetalle, Cu) ───────────────────
        ["silbrig"]        = "#C0C0C0",
        ["silber"]         = "#C0C0C0",
        ["silberweiß"]     = "#E0E0E0",
        ["silbergrau"]     = "#A9A9A9",
        ["gold"]           = "#FFD700",
        ["goldfarben"]     = "#FFD700",
        ["kupfer"]         = "#B87333",
        ["kupferrot"]      = "#B22222",
        ["kupferfarben"]   = "#B87333",
        ["bronze"]         = "#CD7F32",
    };

    // Deduplicated, in Dict-Insertion-Order — für Swatch-Palette in der UI.
    // Aliase mit gleicher CSS-Farbe (z.B. "weiss" / "weiß") werden automatisch gefiltert;
    // die erste Variante aus dem Dictionary gewinnt (Umlaut-Formen stehen dort zuerst).
    private static readonly Lazy<IReadOnlyList<string>> _paletteColors = new(() =>
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var list = new List<string>();
        foreach (var kv in Colors)
        {
            if (seen.Add(kv.Value))
                list.Add(kv.Key);
        }
        return list;
    });

    public static IReadOnlyList<string> PaletteColors => _paletteColors.Value;

    private static string? Normalize(string? colorName) =>
        string.IsNullOrWhiteSpace(colorName) ? null : colorName.Trim();

    /// <summary>
    /// Liefert true, wenn der Farbname im Dictionary vorhanden ist.
    /// Leere Eingaben gelten als "nicht bekannt" (aber nicht als Fehler).
    /// </summary>
    public static bool IsKnown(string? colorName)
    {
        var key = Normalize(colorName);
        return key is not null && Colors.ContainsKey(key);
    }

    /// <summary>
    /// Liefert true, wenn der Wert semantisch "keine Farbe" bedeutet
    /// (keine, nichts, farblos, durchsichtig, klar). Für UI-Darstellung als
    /// Transparenz-Muster (Schachbrett) statt Volltonfarbe.
    /// </summary>
    public static bool IsColorless(string? colorName)
    {
        var key = Normalize(colorName);
        return key is not null && TransparentKeys.Contains(key);
    }

    public static string GetCssColor(string? colorName)
    {
        var key = Normalize(colorName);
        if (key is null) return FallbackColor;
        return Colors.TryGetValue(key, out var color) ? color : FallbackColor;
    }

    public static string GetBorderColor(string? colorName)
    {
        var key = Normalize(colorName);
        if (key is null) return "transparent";
        if (TransparentKeys.Contains(key)) return "transparent";
        return Colors.TryGetValue(key, out var color) ? color : "transparent";
    }
}
