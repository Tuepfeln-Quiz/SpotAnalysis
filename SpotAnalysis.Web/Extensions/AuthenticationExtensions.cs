using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
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
                options.Cookie.MaxAge = TimeSpan.FromMinutes(30);
                options.AccessDeniedPath = "/access-denied";
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

        services.AddHttpContextAccessor();
        services.AddCascadingAuthenticationState();
        services.AddAuthorization();

        services.AddScoped<AuthenticationStateProvider, RevalidatingAuthStateProvider>();

        return services;
    }
}
