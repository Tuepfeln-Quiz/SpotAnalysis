using Microsoft.EntityFrameworkCore;
using SpotAnalysis.Data;

namespace SpotAnalysis.Services.Tests;

[TestFixture]
public abstract class BaseDatabaseTest
{
    private AnalysisContext _context;
    protected IDbContextFactory<AnalysisContext> ContextFactory;

    [OneTimeSetUp]
    public async Task RunBeforeAllTests()
    {
        var options = new DbContextOptionsBuilder<AnalysisContext>()
            .UseSqlServer(TestConfiguration.GetConnectionString("TestDatabase"))
            .Options;
        
        ContextFactory = new TestDbContextFactory(options);

        _context = await ContextFactory.CreateDbContextAsync();

        await _context.Database.EnsureDeletedAsync();
        await _context.Database.MigrateAsync();
        await SeedDatabase();
    }

    [SetUp]
    public Task Init()
    {
        try
        {
            // Optional: Use Respawn here if you want to wipe 
            // user-generated data between tests while keeping seed data.
            _context = new AnalysisContext(new DbContextOptionsBuilder<AnalysisContext>()
                .UseSqlServer(TestConfiguration.GetConnectionString("TestDatabase")).Options);
            return Task.CompletedTask;
        }
        catch (Exception exception)
        {
            return Task.FromException(exception);
        }
    }

    [TearDown]
    public void Cleanup()
    {
        _context.Dispose();
    }
    
    [OneTimeTearDown]
    public void GlobalTeardown()
    {
        _context.Dispose();
    }
    
    private class TestDbContextFactory(DbContextOptions<AnalysisContext> options) : IDbContextFactory<AnalysisContext>
    {
        public AnalysisContext CreateDbContext() => new AnalysisContext(options);
    }

    protected async Task SeedDatabase()
    {
        var seedSql = await File.ReadAllTextAsync(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "seed.sql"));
        if (!string.IsNullOrEmpty(seedSql))
        {
            await _context.Database.ExecuteSqlRawAsync(seedSql);
        }
    }
}