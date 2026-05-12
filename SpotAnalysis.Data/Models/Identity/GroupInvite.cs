namespace SpotAnalysis.Data.Models.Identity;

[Index(nameof(Code), IsUnique = true)]
public class GroupInvite
{
    [Key] public int GroupInviteId { get; set; }

    [StringLength(16)] public required string Code { get; set; } = null!;

    public int GroupId { get; set; }

    [ForeignKey(nameof(GroupId))] public virtual Group Group { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
