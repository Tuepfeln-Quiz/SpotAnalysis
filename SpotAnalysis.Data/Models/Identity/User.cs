namespace SpotAnalysis.Data.Models.Identity;

[Index(nameof(UserName), IsUnique = true)]
public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid UserId { get; set; }

    [StringLength(128)] public required string UserName { get; set; } = null!;

    [StringLength(256)] public required string PasswordHash { get; set; } = null!;

    public DateTime? LastLogin { get; set; }

    public virtual ICollection<Role> Roles { get; set; } = [];
    public virtual ICollection<Group> Groups { get; set; } = [];

    public virtual ICollection<Quiz> Quizzes { get; set; } = [];
    public virtual ICollection<Question> Questions { get; set; } = [];

    public virtual ICollection<Session> Sessions { get; set; } = [];
}
