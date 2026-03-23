namespace Data.Models.Identity; 
public class User {
    [Key]
    public int UserID { get; set; }

    [Required]
    public string UserName { get; set; } = null!;

    [Required]
    public string PasswordHash { get; set; } = null!;

    public virtual ICollection<Role> Roles { get; set; } = [];
    public virtual ICollection<Group> Groups { get; set; } = [];
}
