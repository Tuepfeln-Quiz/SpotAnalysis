namespace SpotAnalysisV2.Web.Models;

// ── Aufgaben-Typ ──────────────────────────────────────────────────────────────

public enum AufgabeTyp { Light, Tuepfeln }

public class AufgabeInfo
{
    public int AufgabeId { get; set; }
    public AufgabeTyp Typ { get; set; }
    public int QuizId { get; set; }
}

// ── Übung ─────────────────────────────────────────────────────────────────────

public class UebungItem
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public List<AufgabeInfo> Aufgaben { get; set; } = new();
    public int AufgabenCount => Aufgaben.Count;
    public int LightCount => Aufgaben.Count(a => a.Typ == AufgabeTyp.Light);
    public int TupfelnCount => Aufgaben.Count(a => a.Typ == AufgabeTyp.Tuepfeln);
}

// ── Quiz-Daten ────────────────────────────────────────────────────────────────

public class QuizData
{
    public List<TuepfelnQuizData> Tuepfeln { get; set; } = new();
    public List<LightQuizData> Light { get; set; } = new();
}

// ── Tüpfeln ───────────────────────────────────────────────────────────────────

public class TuepfelnQuizData
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Beschreibung { get; set; } = "";
    public List<Substanz> Unbekannte { get; set; } = new();
    public List<MischErgebnis> MischErgebnisse { get; set; } = new();
}

public class Substanz
{
    public string Id { get; set; } = "";
    public string Formel { get; set; } = "";
    public string Name { get; set; } = "";
    public string Farbe { get; set; } = "";
    public string FarbeBg { get; set; } = "";
    public string FarbeBorder { get; set; } = "";
    public string PhFarbe { get; set; } = "";
    public string PhWert { get; set; } = "";
    public string Flammenfarbe { get; set; } = "";
    public string FlammenHex { get; set; } = "";
    public string MitNaOH { get; set; } = "";
    public string MitNaOHFarbe { get; set; } = "";
    public string MitHCl { get; set; } = "";
    public string MitHClFarbe { get; set; } = "";
}

public class MischErgebnis
{
    public string[] Ids { get; set; } = Array.Empty<string>();
    public string Farbe { get; set; } = "";
    public string FarbeHex { get; set; } = "";
    public string Text { get; set; } = "";
}

// ── Light ─────────────────────────────────────────────────────────────────────

public class LightQuizData
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public List<LightFrage> Fragen { get; set; } = new();
}

public class LightFrage
{
    public Edukt Edukt1 { get; set; } = new();
    public Edukt CorrectEdukt2 { get; set; } = new();
    public string Produkt { get; set; } = "";
    public string Formel { get; set; } = "";
    public string Frage { get; set; } = "";
    public List<LightOption> Options { get; set; } = new();
}

public class Edukt
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Color { get; set; } = "";
    public string PhLevel { get; set; } = "";
    public string FlameColor { get; set; } = "";
}

public class LightOption
{
    public string Id { get; set; } = "";
    public Edukt Edukt2 { get; set; } = new();
    public string Produkt { get; set; } = "";
    public string Formel { get; set; } = "";
}
