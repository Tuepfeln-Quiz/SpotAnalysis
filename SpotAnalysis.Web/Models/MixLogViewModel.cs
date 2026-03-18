namespace SpotAnalysis.Web.Models;

public class MixLogViewModel
{
    public int LogID { get; set; }
    public string Chemical1Name { get; set; } = "";
    public string Chemical2Name { get; set; } = "";
    public ReactionViewModel? Reaction { get; set; }
    public bool? IsCorrect { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
