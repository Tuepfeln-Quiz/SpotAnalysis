namespace SpotAnalysis.Web.Models;

public class ReactionViewModel
{
    public int ReactionID { get; set; }
    public int Chemical1ID { get; set; }
    public int Chemical2ID { get; set; }
    public string Chemical1Name { get; set; } = "";
    public string Chemical2Name { get; set; } = "";
    public string RelevantProduct { get; set; } = "";
    public string Formula { get; set; } = "";
    public string ObservationDescription { get; set; } = "";
    public string? ImagePath { get; set; }
}
