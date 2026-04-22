using System.ComponentModel.DataAnnotations;

namespace SpotAnalysis.Services.DTOs;

public class ReactionDetailDto
{
    public int Id { get; set; }

    [Required] public int Chemical1Id { get; set; }
    [Required] public int Chemical2Id { get; set; }

    public string Chemical1Name { get; set; } = "";
    public string Chemical2Name { get; set; } = "";

    [Required(ErrorMessage = "Relevantes Produkt ist erforderlich.")]
    [StringLength(256)]
    public string RelevantProduct { get; set; } = "";

    [Required(ErrorMessage = "Formel ist erforderlich.")]
    [StringLength(256)]
    public string Formula { get; set; } = "";

    public int? ObservationId { get; set; }
    public string? NewObservationDescription { get; set; }

    public string ObservationDescription { get; set; } = "";

    [StringLength(256)]
    public string? ImagePath { get; set; }

    public ReferenceReport References { get; set; } = new();
}
