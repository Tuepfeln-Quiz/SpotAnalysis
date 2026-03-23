namespace Models.Identity; 
public class Role {
    [Key]
    public int RoleID { get; set; }

    [Required]
    public string Title { get; set; } = null!;

    public virtual ICollection<UserRole> UserRoles { get; set; } = [];
}
