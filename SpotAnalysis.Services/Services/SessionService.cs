using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpotAnalysis.Data;
using SpotAnalysis.Data.Models.Identity;

namespace SpotAnalysis.Services.Services;

public class SessionService(ILogger<SessionService> logger, IDbContextFactory<AnalysisContext> factory)
    : ISessionService
{
    public async Task<Session> CreateSession(Guid userId, TimeSpan duration, string? userAgent = null,
        string? ipAddress = null)
    {
        await using var context = await factory.CreateDbContextAsync();

        var session = new Session
        {
            UserId = userId,
            Expiry = DateTime.UtcNow.Add(duration),
            UserAgent = userAgent,
            IpAddress = ipAddress,
            StartTime = DateTime.UtcNow,
        };

        await context.Sessions.AddAsync(session);

        await context.SaveChangesAsync();

        logger.LogInformation("Start new session for {UserId}", userId);

        return session;
    }

    public async Task<bool> ValidateSession(Guid sessionId, Guid userId, ISet<string> roles)
    {
        await using var context = await factory.CreateDbContextAsync();

        return await context.Sessions
            .AsNoTracking()
            .AnyAsync(s => s.SessionId == sessionId && s.UserId == userId && s.Expiry > DateTime.UtcNow
                           && roles.All(r => s.User.Roles.Select(role => role.ToString()).Contains(r)));
    }

    public async Task<List<Session>> GetUserSessions(Guid userId)
    {
        await using var context = await factory.CreateDbContextAsync();

        return await context.Sessions.AsNoTracking().Where(s => s.UserId == userId).OrderBy(s => s.SessionId)
            .ToListAsync();
    }

    public async Task<Session> GetSession(Guid sessionId)
    {
        await using var context = await factory.CreateDbContextAsync();

        return await context.Sessions.AsNoTracking().SingleAsync(s => s.SessionId == sessionId);
    }

    public async Task<List<Session>> GetAllSessions()
    {
        await using var context = await factory.CreateDbContextAsync();

        return await context.Sessions.AsNoTracking().OrderBy(s => s.SessionId).ToListAsync();
    }

    public async Task InvalidateForUser(Guid userId)
    {
        await using var context = await factory.CreateDbContextAsync();

        await context.Sessions.Where(s => s.UserId == userId).ExecuteDeleteAsync();
    }

    public async Task InvalidateSession(Guid sessionId)
    {
        await using var context = await factory.CreateDbContextAsync();

        await context.Sessions.Where(s => s.SessionId == sessionId).ExecuteDeleteAsync();
    }

    public async Task CleanupSessions()
    {
        await using var context = await factory.CreateDbContextAsync();

        await context.Sessions.Where(s => s.Expiry < DateTime.UtcNow).ExecuteDeleteAsync();
    }
}
