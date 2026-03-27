namespace SpotAnalysis.Web.Models;

public class ChemicalViewModel
{
    public int ChemicalID { get; set; }
    public string Name { get; set; } = "";
    public string Formula { get; set; } = "";

    public string? ImagePath { get; set; }
    public int ChemicalTypeID { get; set; }
    public string ChemicalTypeName { get; set; } = "";
    public Dictionary<string, string> MethodOutputs { get; set; } = new();
}
