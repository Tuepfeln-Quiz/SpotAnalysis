using SpotAnalysis.Data.Models.Identity;

namespace SpotAnalysis.Services.Services;

public interface ISessionService
{
    Task<Session> CreateSession(Guid userId, TimeSpan duration, string? userAgent, string? ipAddress);
    Task<bool> ValidateSession(Guid sessionId, Guid userId, ISet<string> roles);
    Task<List<Session>> GetUserSessions(Guid userId);
    Task<Session> GetSession(Guid sessionId);
    Task<List<Session>> GetAllSessions();
    Task InvalidateForUser(Guid userId);
    Task InvalidateSession(Guid sessionId);
    Task CleanupSessions();
}
