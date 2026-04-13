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

    public DateTime? LastLogin { get; set; }

    public virtual ISet<Role> Roles { get; set; } = new HashSet<Role>();
    public virtual ICollection<Group> Groups { get; set; } = [];

    public virtual ICollection<Quiz> Quizzes { get; set; } = [];
    public virtual ICollection<Question> Questions {  get; set; } = [];
}