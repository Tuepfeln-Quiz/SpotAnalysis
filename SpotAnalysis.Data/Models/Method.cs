namespace SpotAnalysis.Data.Models;

/// <summary>
/// Contains methods such as "ph paper" that can be applied to chemicals to produce an observation (color). The output from such a usecase is stored in the MethodOutput table.
/// </summary>

public class Method {
    [Key]
    public int MethodID { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    public virtual ICollection<MethodOutput> MethodOutputs { get; set; } = [];
    public virtual ICollection<STAvailableMethod> STAvailableMethods { get; set; } = [];
}
