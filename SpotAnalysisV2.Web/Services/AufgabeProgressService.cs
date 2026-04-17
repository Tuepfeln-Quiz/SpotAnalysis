namespace SpotAnalysisV2.Web.Services;

public class AufgabeProgressService
{
    private readonly Dictionary<(int UebungId, int AufgabeId), bool> _results = new();

    // Navigation context set by Aufgabe.razor before routing to a quiz
    public int CurrentUebungId { get; private set; }
    public int CurrentAufgabeId { get; private set; }
    public int CurrentTotal { get; private set; }
    public bool IsInUebung => CurrentTotal > 0;

    public void SetUebungContext(int uebungId, int aufgabeId, int total)
    {
        CurrentUebungId = uebungId;
        CurrentAufgabeId = aufgabeId;
        CurrentTotal = total;
    }

    public void RecordResult(int uebungId, int aufgabeId, bool passed)
        => _results[(uebungId, aufgabeId)] = passed;

    public bool? GetResult(int uebungId, int aufgabeId)
        => _results.TryGetValue((uebungId, aufgabeId), out var r) ? r : null;
}
