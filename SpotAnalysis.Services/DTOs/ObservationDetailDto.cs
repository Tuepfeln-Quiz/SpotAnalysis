using System.ComponentModel.DataAnnotations;

namespace SpotAnalysis.Services.DTOs;

public class ObservationDetailDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Beschreibung ist erforderlich.")]
    [StringLength(512)]
    public string Description { get; set; } = "";

    public ReferenceReport References { get; set; } = new();
}
