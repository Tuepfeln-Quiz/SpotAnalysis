using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using SpotAnalysis.Services.Services;

namespace SpotAnalysis.Web.Services;

public class RevalidatingAuthStateProvider(
    ILoggerFactory loggerFactory,
    IServiceScopeFactory scopeFactory)
    : RevalidatingServerAuthenticationStateProvider(loggerFactory)
{
    protected override TimeSpan RevalidationInterval => TimeSpan.FromSeconds(30);

    protected override async Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState, CancellationToken cancellationToken)
    {
        var sidClaim = authenticationState.User.FindFirstValue(ClaimTypes.Sid);

        if (sidClaim is null || !Guid.TryParse(sidClaim, out var sessionId))
            return false;

        var userIdClaim = authenticationState.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
            return false;

        var currentRoles = authenticationState.User.FindAll(ClaimTypes.Role)
            .Select(c => c.Value).ToHashSet();

        await using var scope = scopeFactory.CreateAsyncScope();

        var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();

        return await sessionService.ValidateSession(sessionId, userId, currentRoles);
    }
}
