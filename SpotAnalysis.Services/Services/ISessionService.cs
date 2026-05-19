using SpotAnalysis.Data.Models.Identity;

namespace SpotAnalysis.Services.Services;

public interface ISessionService
{
    /// <summary>
    /// Create a new session for the user
    /// </summary>
    /// <param name="userId">The users Guid</param>
    /// <param name="duration">The duration of the session, should be positive</param>
    /// <param name="userAgent">Optional binding of the session, currently unused</param>
    /// <param name="ipAddress">Optional binding of the session, currently unused</param>
    /// <returns></returns>
    Task<Session> CreateSession(Guid userId, TimeSpan duration, string? userAgent = null, string? ipAddress = null);

    /// <summary>
    /// Checks the sessions expiry, user assignment and role consistency
    /// </summary>
    /// <param name="sessionId">The session Guid</param>
    /// <param name="userId">The session users Guid</param>
    /// <param name="roles">The roles of the users session claim</param>
    /// <returns></returns>
    Task<bool> ValidateSession(Guid sessionId, Guid userId, ISet<string> roles);

    /// <summary>
    /// Returns all sessions for the given user
    /// </summary>
    /// <param name="userId">The users Guid</param>
    /// <returns></returns>
    Task<List<Session>> GetUserSessions(Guid userId);

    /// <summary>
    /// Returns a rich session
    /// </summary>
    /// <param name="sessionId">The session Guid</param>
    /// <returns></returns>
    Task<Session> GetSession(Guid sessionId);

    /// <summary>
    /// Returns all active sessions
    /// </summary>
    /// <returns></returns>
    Task<List<Session>> GetAllSessions();

    /// <summary>
    /// Invalidate all sessions for the given user
    /// </summary>
    /// <param name="userId">The users Guid</param>
    /// <returns></returns>
    Task InvalidateAllForUser(Guid userId);

    /// <summary>
    /// Invalidates a session
    /// </summary>
    /// <param name="sessionId">The session Guid</param>
    /// <returns></returns>
    Task InvalidateSession(Guid sessionId);

    /// <summary>
    /// Deletes all expired session entries from the database
    /// </summary>
    /// <returns></returns>
    Task CleanupSessions();
}
