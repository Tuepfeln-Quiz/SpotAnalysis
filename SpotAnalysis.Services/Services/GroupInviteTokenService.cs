using Microsoft.AspNetCore.DataProtection;

namespace SpotAnalysis.Services.Services;

public class GroupInviteTokenService : IGroupInviteTokenService
{
    private const string Purpose = "GroupInvite.v1";
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromMinutes(15);

    private readonly ITimeLimitedDataProtector _protector;

    public GroupInviteTokenService(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector(Purpose).ToTimeLimitedDataProtector();
    }

    public string CreateToken(int groupId)
    {
        return _protector.Protect(groupId.ToString(), TokenLifetime);
    }

    public int? ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        try
        {
            var payload = _protector.Unprotect(token);
            return int.TryParse(payload, out var groupId) ? groupId : null;
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            return null;
        }
    }
}
