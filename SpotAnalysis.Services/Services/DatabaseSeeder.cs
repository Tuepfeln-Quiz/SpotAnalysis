using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpotAnalysis.Data;
using SpotAnalysis.Data.Enums;
using SpotAnalysis.Data.Models.Identity;

namespace SpotAnalysis.Services.Services;

public class DatabaseSeeder(
    ILogger<DatabaseSeeder> logger,
    IDbContextFactory<AnalysisContext> factory) : IDatabaseSeeder
{
    private const string DevPassword = "!Password1";

    private static readonly (string UserName, Guid UserId, Role[] Roles)[] DevUsers =
    [
        ("Lehrer1",   new Guid("11111111-1111-1111-1111-111111111111"), [Role.Student, Role.Teacher]),
        ("Lehrer2",   new Guid("22222222-2222-2222-2222-222222222222"), [Role.Student, Role.Teacher]),
        ("Lehrer3",   new Guid("33333333-3333-3333-3333-333333333333"), [Role.Student, Role.Teacher]),
        ("Schueler1", new Guid("44444444-4444-4444-4444-444444444444"), [Role.Student]),
        ("Schueler2", new Guid("55555555-5555-5555-5555-555555555555"), [Role.Student]),
        ("Schueler3", new Guid("66666666-6666-6666-6666-666666666666"), [Role.Student]),
    ];

    public async Task SeedDevUserAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);

        var devUserNames = DevUsers.Select(u => u.UserName).ToArray();
        var existing = await context.Users
            .Where(u => devUserNames.Contains(u.UserName))
            .Select(u => u.UserName)
            .ToListAsync(cancellationToken);

        var added = 0;
        foreach (var (userName, userId, roles) in DevUsers)
        {
            if (existing.Contains(userName)) continue;

            var user = new User
            {
                UserID = userId,
                UserName = userName,
                PasswordHash = new PasswordProvider.Password(DevPassword, userId).ParamString(),
            };
            foreach (var role in roles) user.Roles.Add(role);
            context.Users.Add(user);
            added++;
        }

        if (added == 0)
        {
            logger.LogInformation("Dev data seeder: all {Total} users already present", DevUsers.Length);
            return;
        }

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Dev data seeder added {Count}/{Total} users", added, DevUsers.Length);
    }

    public async Task SeedAdminAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);

        var user = await context.Users.FirstOrDefaultAsync(u => u.UserName == "Admin", cancellationToken);
        if(user != null)
        {
            return;
        }

        var newGuid = Guid.NewGuid();
        var adminUser = new User
        {
            UserID = newGuid,
            UserName = "Admin",
            PasswordHash = new PasswordProvider.Password("admin123", newGuid).ParamString(),
        };
        adminUser.Roles.Add(Role.Admin);
        context.Users.Add(adminUser);
        await context.SaveChangesAsync(cancellationToken);
    }
}
