namespace SpotAnalysis.Data.Models.Identity;

[Index(nameof(UserName), IsUnique = true)]
public class User {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid UserID { get; set; }

    [Required]
    public string UserName { get; set; } = null!;

    [Required]
    public string PasswordHash { get; set; } = null!;

    public virtual ICollection<Role> Roles { get; set; } = [];
    public virtual ICollection<Group> Groups { get; set; } = [];
}