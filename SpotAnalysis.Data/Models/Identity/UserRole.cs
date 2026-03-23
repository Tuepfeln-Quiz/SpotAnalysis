namespace Models.Identity;

[PrimaryKey(nameof(UserID), nameof(RoleID))]
public class UserRole {
    public int UserID { get; set; }
    public int RoleID { get; set; }


    [DeleteBehavior(DeleteBehavior.Restrict)]
    public User User { get; set; } = null!;
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Role Role { get; set; } = null!;
}
