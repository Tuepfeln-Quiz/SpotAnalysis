namespace Data.Models.Identity;

[Index(nameof(Name), IsUnique = true)]
public class Group {
    [Key]
    public int GroupID { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Quiz> Quizzes { get; set; } = [];
    public virtual ICollection<User> Users { get; set; } = [];
}
