namespace SpotAnalysis.Data.Models.Identity;

[Index(nameof(Name), IsUnique = true)]
public class Group
{
    [Key]
    public int GroupID { get; set; }

    [Required]
    [StringLength(maximumLength: 64, MinimumLength = 4)]
    public string Name { get; set; } = null!;

    [StringLength(512)]
    public string? Description { get; set; }

    public virtual ICollection<Quiz> Quizzes { get; set; } = [];
    public virtual ICollection<User> Users { get; set; } = [];
}
