namespace Data.Models.Identity;

public class Group {
    [Key]
    public int GroupID { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<GroupQuiz> GroupQuizzes { get; set; } = [];
    public virtual ICollection<UserGroup> UserGroups { get; set; } = [];
}
