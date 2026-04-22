using System.ComponentModel.DataAnnotations;
using SpotAnalysis.Data.Enums;

namespace SpotAnalysis.Services.DTOs;

public class ChemicalDetailDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name ist erforderlich.")]
    [StringLength(256)]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Formel ist erforderlich.")]
    [StringLength(256)]
    public string Formula { get; set; } = "";

    [Required(ErrorMessage = "Eigenfarbe ist erforderlich.")]
    [StringLength(128)]
    public string Color { get; set; } = "";

    [Required]
    public ChemicalType Type { get; set; }

    [StringLength(256)]
    public string? ImagePath { get; set; }

    public List<MethodOutputEntry> MethodOutputs { get; set; } = new();

    public ReferenceReport References { get; set; } = new();
}
