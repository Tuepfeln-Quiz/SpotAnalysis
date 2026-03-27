using Microsoft.EntityFrameworkCore;
using SpotAnalysis.Data;

namespace SpotAnalysis.Services.Tests;

[TestFixture]
public abstract class BaseDatabaseTest
{
    private AnalysisContext _context;
    protected IDbContextFactory<AnalysisContext> ContextFactory;

    private const string ConnectionStringIntegrated =
        @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=TuepfelnTest;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False;Command Timeout=30";

    // ReSharper disable once UnusedMember.Local
    private const string ConnectionStringLocal =
        @"Server=tcp:localhost,1433;Database=TuepfelnTest;User Id=sa;Password=p4ssw0rd!;TrustServerCertificate=True;Encrypt=False;";
    
    [OneTimeSetUp]
    public async Task RunBeforeAllTests()
    {
        var options = new DbContextOptionsBuilder<AnalysisContext>()
            .UseSqlServer(ConnectionStringIntegrated)
            .Options;
        
        ContextFactory = new TestDbContextFactory(options);

        _context = await ContextFactory.CreateDbContextAsync();

        await _context.Database.EnsureDeletedAsync();
        await _context.Database.MigrateAsync();

        var seedSql = await File.ReadAllTextAsync(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "seed.sql"));
        if (!string.IsNullOrEmpty(seedSql))
        {
            await _context.Database.ExecuteSqlRawAsync(seedSql);
        }
    }

    [SetUp]
    public Task Init()
    {
        try
        {
            // Optional: Use Respawn here if you want to wipe 
            // user-generated data between tests while keeping seed data.
            _context = new AnalysisContext(new DbContextOptionsBuilder<AnalysisContext>()
                .UseSqlServer(ConnectionStringIntegrated).Options);
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
}