using DataAccessLayer.Models.SpotTest;

namespace DataAccessLayer.Models; 
public class Method {
    [Key]
    public int MethodID { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    public virtual ICollection<MethodOutput> MethodOutputs { get; set; } = [];
    public virtual ICollection<STAvailableMethod> STAvailableMethods { get; set; } = [];
}
