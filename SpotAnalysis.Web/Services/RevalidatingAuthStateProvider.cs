using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.EntityFrameworkCore;
using SpotAnalysis.Data;

namespace SpotAnalysis.Web.Services;

public class RevalidatingAuthStateProvider(
    ILoggerFactory loggerFactory,
    IDbContextFactory<AnalysisContext> dbFactory)
    : RevalidatingServerAuthenticationStateProvider(loggerFactory)
{
    protected override TimeSpan RevalidationInterval => TimeSpan.FromSeconds(30);

    protected override async Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState, CancellationToken cancellationToken)
    {
        var userIdClaim = authenticationState.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
            return false;

        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);

        var dbRoles = await db.Users
            .AsNoTracking()
            .Where(u => u.UserID == userId)
            .SelectMany(u => u.Roles)
            .ToListAsync(cancellationToken);

        var currentRoles = authenticationState.User.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToHashSet();
        var freshRoles = dbRoles.Select(r => r.ToString()).ToHashSet();

        return currentRoles.SetEquals(freshRoles);
    }
}
