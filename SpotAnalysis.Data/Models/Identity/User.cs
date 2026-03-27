namespace SpotAnalysis.Data.Models.Identity;

[Index(nameof(UserName), IsUnique = true)]
public class User {
    [Key]
    [StringLength(36, MinimumLength = 36)]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string UserID { get; set; } = null!;

    [Required]
    public string UserName { get; set; } = null!;

    [Required]
    public string PasswordHash { get; set; } = null!;

    public virtual ICollection<Role> Roles { get; set; } = [];
    public virtual ICollection<Group> Groups { get; set; } = [];
}
