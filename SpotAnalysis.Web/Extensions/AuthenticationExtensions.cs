using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using SpotAnalysis.Services.Services;
using SpotAnalysis.Web.Services;

namespace SpotAnalysis.Web.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddWebAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = "auth_token";
                options.LoginPath = "/login";
                options.Cookie.MaxAge = TimeSpan.FromDays(7);
                options.AccessDeniedPath = "/access-denied";
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

                options.Events.OnValidatePrincipal = ValidateSessionCookie;
            });

        services.AddHttpContextAccessor();
        services.AddCascadingAuthenticationState();
        services.AddAuthorization();

        services.AddScoped<AuthenticationStateProvider, RevalidatingAuthStateProvider>();

        return services;
    }

    private static async Task ValidateSessionCookie(CookieValidatePrincipalContext context)
    {
        var principal = context.Principal;
        if (principal is null)
            return;

        var sidClaim = principal.FindFirstValue(ClaimTypes.Sid);
        if (sidClaim is null || !Guid.TryParse(sidClaim, out var sessionId))
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync();
            return;
        }

        var userIdClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync();
            return;
        }

        var roles = principal.FindAll(ClaimTypes.Role)
            .Select(c => c.Value).ToHashSet();

        var sessionService = context.HttpContext.RequestServices.GetRequiredService<ISessionService>();
        var isValid = await sessionService.ValidateSession(sessionId, userId, roles);

        if (!isValid)
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync();
        }
    }
}
