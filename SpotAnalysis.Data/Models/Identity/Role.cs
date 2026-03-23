namespace SpotAnalysis.Data.Models.Identity; 
public class Role {
    [Key]
    public int RoleID { get; set; }

    [Required]
    public string Title { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = [];
}
