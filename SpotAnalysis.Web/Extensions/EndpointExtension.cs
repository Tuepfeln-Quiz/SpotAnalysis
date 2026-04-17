using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SpotAnalysis.Services.Services;
using System.Security.Claims;

namespace SpotAnalysis.Web.Extensions;

public static class EndpointExtensions {
    public static WebApplication MapAuthEndpoints(this WebApplication app) {
        app.MapPost("/api/auth/login", async (HttpContext context, [FromForm] string userName, [FromForm] string password, IUserService userService) => {
            try {
                var user = await userService.Login(userName, password);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString())
                };

                // Add roles if applicable
                foreach (var role in user.Roles) {
                    claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
                }

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                return Results.Redirect("/");
            } catch (Exception) {
                return Results.Redirect("/login?error=InvalidCredentials");
            }
        });

        app.MapPost("/api/auth/logout", async (HttpContext context) => {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Redirect("/login");
        });

        return app;
    }
}