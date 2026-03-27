namespace SpotAnalysis.Data.Models.Identity;

[Index(nameof(Title), IsUnique = true)]
public class Role {
    [Key]
    public int RoleID { get; set; }

    [Required]
    public string Title { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = [];
}