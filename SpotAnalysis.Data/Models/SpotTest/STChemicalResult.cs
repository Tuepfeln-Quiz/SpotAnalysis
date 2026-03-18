using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer.Models.SpotTest;

public class STChemicalResult {
    [Key]
    public int ChemicalResultID { get; set; }
    public int ResultID { get; set; }
    public int SelectedChemicalID { get; set; }
    public int ChosenChemicalID { get; set; }


    [ForeignKey(nameof(ResultID))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public STResult Result { get; set; } = null!;

    [ForeignKey(nameof(SelectedChemicalID))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Chemical SelectedChemical { get; set; } = null!;

    [ForeignKey(nameof(ChosenChemicalID))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Chemical ChosenChemical { get; set; } = null!;
}