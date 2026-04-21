namespace SpotAnalysis.Services.DTOs;

public class ImportResult
{
    public int ChemicalsAdded { get; set; }
    public int ChemicalsUpdated { get; set; }
    public int ChemicalsSkipped { get; set; }
    public int ReactionsAdded { get; set; }
    public int ReactionsUpdated { get; set; }
    public int ReactionsSkipped { get; set; }
    public int ObservationsAdded { get; set; }
    public int ObservationsSkipped { get; set; }
}
