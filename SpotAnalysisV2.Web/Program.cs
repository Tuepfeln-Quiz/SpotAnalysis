using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SpotAnalysis.Services;
using SpotAnalysis.Services.Services;
using SpotAnalysisV2.Web.Components;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSpotAnalysis(builder.Configuration);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
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

builder.Services.AddHttpContextAccessor();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateAsyncScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();
    try
    {
        await seeder.SeedAdminAsync();
        await seeder.SeedMasterDataAsync();

        if (app.Environment.IsDevelopment())
        {
            await seeder.SeedDevUserAsync();
            await seeder.SeedQuizDataAsync();
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Seeding failed");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
MapAuthEndpoints(app);
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

static WebApplication MapAuthEndpoints(WebApplication app)
{
    app.MapPost("/api/auth/login", async (HttpContext context, [FromForm] string userName, [FromForm] string password, [FromForm] string? returnUrl, IUserService userService) =>
    {
        try
        {
            var user = await userService.Login(userName, password);

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.NameIdentifier, user.UserID.ToString())
            };

            foreach (var role in user.Roles)
                claims.Add(new Claim(ClaimTypes.Role, role.ToString()));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            var redirect = IsLocalUrl(returnUrl) ? returnUrl : "/";
            return Results.Redirect(redirect);
        }
        catch (Exception)
        {
            return Results.Redirect("/login?error=InvalidCredentials");
        }
    });

    app.MapPost("/api/auth/register", async (HttpContext context, [FromForm] string userName, [FromForm] string password, [FromForm] string confirmPassword, [FromForm] string? returnUrl, IUserService userService) =>
    {
        try
        {
            if (password != confirmPassword)
                return Results.Redirect(BuildRegisterErrorUrl("PasswordsDoNotMatch", returnUrl));

            await userService.Register(userName, password);
            var user = await userService.Login(userName, password);

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.NameIdentifier, user.UserID.ToString())
            };

            foreach (var role in user.Roles)
                claims.Add(new Claim(ClaimTypes.Role, role.ToString()));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            var redirect = IsLocalUrl(returnUrl) ? returnUrl : "/";
            return Results.Redirect(redirect);
        }
        catch (ArgumentException ex) when (ex.Message is "InvalidUserName" or "WeakPassword")
        {
            return Results.Redirect(BuildRegisterErrorUrl(ex.Message, returnUrl));
        }
        catch (InvalidOperationException ex) when (ex.Message == "UserNameTaken")
        {
            return Results.Redirect(BuildRegisterErrorUrl(ex.Message, returnUrl));
        }
        catch (Exception)
        {
            return Results.Redirect(BuildRegisterErrorUrl("RegistrationFailed", returnUrl));
        }
    });

    app.MapPost("/api/auth/profile", async (HttpContext context, [FromForm] string userName, [FromForm] string? newPassword, [FromForm] string? confirmPassword, [FromForm] string? avatarEmoji, [FromForm] string? returnUrl, IUserService userService) =>
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? context.User.FindFirst("sub")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Results.Redirect(BuildProfileUrl("UserNotFound", returnUrl));

        var normalizedNewPassword = newPassword?.Trim();
        var normalizedConfirmPassword = confirmPassword?.Trim();

        var wantsPasswordChange = !string.IsNullOrEmpty(normalizedNewPassword) || !string.IsNullOrEmpty(normalizedConfirmPassword);
        if (wantsPasswordChange && (string.IsNullOrEmpty(normalizedNewPassword) || string.IsNullOrEmpty(normalizedConfirmPassword) || normalizedNewPassword != normalizedConfirmPassword))
            return Results.Redirect(BuildProfileUrl("PasswordsDoNotMatch", returnUrl));

        try
        {
            var user = await userService.UpdateProfile(userId, userName, wantsPasswordChange ? normalizedNewPassword : null);

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.NameIdentifier, user.UserID.ToString())
            };

            foreach (var role in user.Roles)
                claims.Add(new Claim(ClaimTypes.Role, role.ToString()));

            if (!string.IsNullOrWhiteSpace(avatarEmoji))
                claims.Add(new Claim("avatar", avatarEmoji));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            return Results.Redirect(BuildProfileSuccessUrl(returnUrl));
        }
        catch (ArgumentException ex) when (ex.Message is "InvalidUserName" or "WeakPassword")
        {
            return Results.Redirect(BuildProfileUrl(ex.Message, returnUrl));
        }
        catch (InvalidOperationException ex) when (ex.Message is "UserNameTaken" or "UserNotFound")
        {
            return Results.Redirect(BuildProfileUrl(ex.Message, returnUrl));
        }
        catch (Exception)
        {
            return Results.Redirect(BuildProfileUrl("ProfileUpdateFailed", returnUrl));
        }
    });

    app.MapPost("/api/auth/logout", async (HttpContext context) =>
    {
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Results.Redirect("/login");
    });

    return app;
}

static bool IsLocalUrl(string? url)
    => !string.IsNullOrEmpty(url) && url.StartsWith('/') && !url.StartsWith("//");

static string BuildRegisterErrorUrl(string error, string? returnUrl)
{
    var encodedError = Uri.EscapeDataString(error);
    if (!IsLocalUrl(returnUrl))
        return $"/register?error={encodedError}";

    return $"/register?error={encodedError}&returnUrl={Uri.EscapeDataString(returnUrl)}";
}

static string BuildProfileUrl(string error, string? returnUrl)
{
    var path = IsLocalUrl(returnUrl) ? returnUrl! : "/profile";
    var separator = path.Contains('?') ? '&' : '?';
    return $"{path}{separator}error={Uri.EscapeDataString(error)}";
}

static string BuildProfileSuccessUrl(string? returnUrl)
{
    var path = IsLocalUrl(returnUrl) ? returnUrl! : "/profile";
    var separator = path.Contains('?') ? '&' : '?';
    return $"{path}{separator}success=ProfileUpdated";
}
