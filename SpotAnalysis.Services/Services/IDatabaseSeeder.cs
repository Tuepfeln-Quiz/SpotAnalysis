namespace SpotAnalysis.Services.Services;

public interface IDatabaseSeeder
{
    Task SeedDevUserAsync(CancellationToken cancellationToken = default);
    Task SeedAdminAsync(CancellationToken cancellationToken = default);
}
