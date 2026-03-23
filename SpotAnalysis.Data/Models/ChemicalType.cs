namespace Models; 
public class ChemicalType {
    [Key]
    public int ChemicalTypeID { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    public virtual ICollection<Chemical> Chemicals { get; set; } = [];
}
