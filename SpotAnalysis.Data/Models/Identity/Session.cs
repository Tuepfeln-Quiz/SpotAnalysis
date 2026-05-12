namespace SpotAnalysis.Data.Models.Identity;

[Table("Sessions")]
public class Session
{
    [Key] public Guid SessionId { get; set; }

    public Guid UserId { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime Expiry { get; set; }

    [StringLength(512)] public string? UserAgent { get; set; }

    [StringLength(46)] public string? IpAddress { get; set; }

    public virtual User User { get; set; } = null!;
}
