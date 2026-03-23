namespace Models.Identity; 
public class User {
    [Key]
    public int UserID { get; set; }

    [Required]
    public string UserName { get; set; } = null!;

    [Required]
    public string PasswordHash { get; set; } = null!;

    public virtual ICollection<UserRole> UserRoles { get; set; } = [];
}
