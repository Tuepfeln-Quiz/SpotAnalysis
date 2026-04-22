namespace SpotAnalysis.Data.Models.Identity;

[Index(nameof(Code), IsUnique = true)]
public class GroupInvite
{
    [Key]
    public int GroupInviteID { get; set; }

    [Required]
    [StringLength(16)]
    public string Code { get; set; } = null!;

    public int GroupID { get; set; }

    [ForeignKey(nameof(GroupID))]
    public virtual Group Group { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
