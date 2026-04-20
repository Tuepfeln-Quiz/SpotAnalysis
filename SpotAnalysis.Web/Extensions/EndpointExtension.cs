using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SpotAnalysis.Services.Services;
using System.Security.Claims;

namespace SpotAnalysis.Web.Extensions;

public static class EndpointExtensions {
    public static WebApplication MapAuthEndpoints(this WebApplication app) {
        app.MapPost("/api/auth/login", async (HttpContext context, [FromForm] string userName, [FromForm] string password, [FromForm] string? returnUrl, IUserService userService) => {
            try {
                var user = await userService.Login(userName, password);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString())
                };

                foreach (var role in user.Roles) {
                    claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
                }

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                var redirect = IsLocalUrl(returnUrl) ? returnUrl : "/";
                return Results.Redirect(redirect);
            } catch (Exception) {
                return Results.Redirect("/login?error=InvalidCredentials");
            }
        });

        app.MapPost("/api/auth/register", async (HttpContext context, [FromForm] string userName, [FromForm] string password, [FromForm] string confirmPassword, [FromForm] string? returnUrl, IUserService userService) => {
            try {
                if (password != confirmPassword) {
                    return Results.Redirect(BuildRegisterErrorUrl("PasswordsDoNotMatch", returnUrl));
                }

                await userService.Register(userName, password);

                var user = await userService.Login(userName, password);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString())
                };

                foreach (var role in user.Roles) {
                    claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
                }

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                var redirect = IsLocalUrl(returnUrl) ? returnUrl : "/";

                return Results.Redirect(redirect);
            } catch (ArgumentException ex) when (ex.Message is "InvalidUserName" or "WeakPassword") {
                return Results.Redirect(BuildRegisterErrorUrl(ex.Message, returnUrl));
            } catch (InvalidOperationException ex) when (ex.Message == "UserNameTaken") {
                return Results.Redirect(BuildRegisterErrorUrl(ex.Message, returnUrl));
            } catch (Exception) {
                return Results.Redirect(BuildRegisterErrorUrl("RegistrationFailed", returnUrl));
            }
        });

        app.MapPost("/api/auth/logout", async (HttpContext context) => {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Redirect("/login");
        });

        return app;
    }

    private static bool IsLocalUrl(string? url)
        => !string.IsNullOrEmpty(url) && url.StartsWith('/') && !url.StartsWith("//");

    private static string BuildRegisterErrorUrl(string error, string? returnUrl) {
        var encodedError = Uri.EscapeDataString(error);
        if (!IsLocalUrl(returnUrl)) {
            return $"/register?error={encodedError}";
        }

        return $"/register?error={encodedError}&returnUrl={Uri.EscapeDataString(returnUrl)}";
    }
}