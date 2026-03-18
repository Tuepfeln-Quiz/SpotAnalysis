namespace Data.Models.Identity;

[PrimaryKey(nameof(UserID), nameof(GroupID))]
public class UserGroup {
    public int UserID { get; set; }
    public int GroupID { get; set; }


    [DeleteBehavior(DeleteBehavior.Restrict)]
    public User User { get; set; } = null!;

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Group Group { get; set; } = null!;
}
