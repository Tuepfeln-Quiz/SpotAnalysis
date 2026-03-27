using Microsoft.EntityFrameworkCore;
using SpotAnalysis.Data;

namespace SpotAnalysis.Services.Tests;

[TestFixture]
public abstract class BaseDatabaseTest
{
    protected AnalysisContext Context;
    protected IDbContextFactory<AnalysisContext> ContextFactory;

    private const string ConnectionString =
        @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=TuepfelnTest;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False;Command Timeout=30";
    
    [OneTimeSetUp]
    public async Task RunBeforeAllTests()
    {
        var options = new DbContextOptionsBuilder<AnalysisContext>()
            .UseSqlServer(ConnectionString)
            .Options;
        
        ContextFactory = new TestDbContextFactory(options);

        Context = await ContextFactory.CreateDbContextAsync();

        await Context.Database.EnsureDeletedAsync();
        await Context.Database.MigrateAsync();

        var seedSql = await File.ReadAllTextAsync(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "seed.sql"));
        if (!string.IsNullOrEmpty(seedSql))
        {
            await Context.Database.ExecuteSqlRawAsync(seedSql);
        }
    }

    [SetUp]
    public async Task Init()
    {
        // Optional: Use Respawn here if you want to wipe 
        // user-generated data between tests while keeping seed data.
        Context = new AnalysisContext(new DbContextOptionsBuilder<AnalysisContext>()
            .UseSqlServer(ConnectionString).Options);
    }

    [TearDown]
    public void Cleanup()
    {
        Context.Dispose();
    }
    
    [OneTimeTearDown]
    public void GlobalTeardown()
    {
        Context?.Dispose();
    }
    
    private class TestDbContextFactory(DbContextOptions<AnalysisContext> options) : IDbContextFactory<AnalysisContext>
    {
        public AnalysisContext CreateDbContext() => new AnalysisContext(options);
    }
}