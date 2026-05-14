using SpotAnalysis.Data.Models.Identity;

namespace SpotAnalysis.Services.Services;

public interface IGroupInviteTokenService
{
    Task<string> CreateToken(int groupId);

    /// <summary>
    /// Looks up an invite by code. Returns the entity if found, null otherwise.
    /// Does not check expiry — callers decide how to handle that.
    /// </summary>
    Task<GroupInvite?> ValidateToken(string token);
}
