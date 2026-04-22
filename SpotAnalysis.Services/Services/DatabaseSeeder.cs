using ExcelImportExport.Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpotAnalysis.Data;
using SpotAnalysis.Data.Enums;
using SpotAnalysis.Data.Models.Identity;

namespace SpotAnalysis.Services.Services;

public class DatabaseSeeder(
    ILogger<DatabaseSeeder> logger,
    IDbContextFactory<AnalysisContext> factory,
    IXlsImportExportService xlsImportExportService) : IDatabaseSeeder
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
        if (user != null)
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

    public async Task SeedMasterDataAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);

        if (await context.Chemicals.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Stammdaten-Seed übersprungen: Chemicals bereits vorhanden.");
            return;
        }

        await using var stream = GetEmbeddedResource("SpotAnalysis.Services.SeedData.Stammdaten.xlsx");
        await xlsImportExportService.ImportFromStreamAsync(stream, ExcelFormat.Xlsx);

        logger.LogInformation(
            "Stammdaten-Seed eingespielt: {Chemicals} Chemicals, {Reactions} Reactions, {Methods} Methods, {Observations} Observations.",
            await context.Chemicals.CountAsync(cancellationToken),
            await context.Reactions.CountAsync(cancellationToken),
            await context.Methods.CountAsync(cancellationToken),
            await context.Observations.CountAsync(cancellationToken));
    }

    public async Task SeedQuizDataAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);

        // Precondition: Stammdaten (Reactions, Chemicals, Methods) müssen existieren.
        // Ohne sie würden die FK-Referenzen im Skript scheitern.
        var hasReactions = await context.Reactions.AnyAsync(cancellationToken);
        var hasChemicals = await context.Chemicals.AnyAsync(cancellationToken);
        var hasMethods = await context.Methods.AnyAsync(cancellationToken);
        if (!hasReactions || !hasChemicals || !hasMethods)
        {
            logger.LogWarning(
                "Quiz-Seed übersprungen: Stammdaten fehlen (Reactions={HasReactions}, Chemicals={HasChemicals}, Methods={HasMethods}).",
                hasReactions, hasChemicals, hasMethods);
            return;
        }

        var script = await ReadEmbeddedTextAsync("SpotAnalysis.Services.Scripts.SeedQuizData.sql", cancellationToken);
        await context.Database.ExecuteSqlRawAsync(script, cancellationToken);
        logger.LogInformation("Quiz-Seed erfolgreich eingespielt.");
    }

    private static Stream GetEmbeddedResource(string resourceName)
    {
        var assembly = typeof(DatabaseSeeder).Assembly;
        return assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' not found.");
    }

    private static async Task<string> ReadEmbeddedTextAsync(string resourceName, CancellationToken cancellationToken)
    {
        await using var stream = GetEmbeddedResource(resourceName);
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(cancellationToken);
    }
}
