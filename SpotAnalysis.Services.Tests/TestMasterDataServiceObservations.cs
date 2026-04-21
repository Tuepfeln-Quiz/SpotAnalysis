using Microsoft.EntityFrameworkCore;
using SpotAnalysis.Services.DTOs;
using SpotAnalysis.Services.Services;

namespace SpotAnalysis.Services.Tests;

[TestFixture]
public class TestMasterDataServiceObservations : BaseDatabaseTest
{
    [SetUp]
    public async Task Reset() => await CleanUpDb();

    [Test]
    public async Task GetObservationsAsync_ReturnsAll()
    {
        var service = new MasterDataService(ContextFactory);
        var result = await service.GetObservationsAsync();
        Assert.That(result, Is.Not.Empty);
    }

    [Test]
    public async Task CreateObservationAsync_Inserts()
    {
        var service = new MasterDataService(ContextFactory);
        var id = await service.CreateObservationAsync(new ObservationDetailDto
        {
            Description = "Test-Beobachtung"
        });
        Assert.That(id, Is.GreaterThan(0));
    }

    [Test]
    public async Task CreateObservationAsync_Throws_OnDuplicateDescription()
    {
        var service = new MasterDataService(ContextFactory);
        await using var context = await ContextFactory.CreateDbContextAsync();
        var existing = await context.Observations.AsNoTracking().FirstAsync();

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.CreateObservationAsync(new ObservationDetailDto
            {
                Description = existing.Description.ToUpperInvariant()
            }));
    }

    [Test]
    public async Task UpdateObservationAsync_PersistsChanges()
    {
        var service = new MasterDataService(ContextFactory);
        var id = await service.CreateObservationAsync(new ObservationDetailDto { Description = "Alt" });

        await service.UpdateObservationAsync(new ObservationDetailDto { Id = id, Description = "Neu" });

        var list = await service.GetObservationsAsync();
        Assert.That(list.Any(o => o.Id == id && o.Description == "Neu"), Is.True);
    }

    [Test]
    public async Task DeleteObservationAsync_Throws_WhenReferencedByReaction()
    {
        var service = new MasterDataService(ContextFactory);
        await using var context = await ContextFactory.CreateDbContextAsync();
        var inUse = await context.Observations
            .Where(o => context.Reactions.Any(r => r.ObservationID == o.ObservationID))
            .FirstAsync();

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.DeleteObservationAsync(inUse.ObservationID));
    }

    [Test]
    public async Task DeleteObservationAsync_RemovesUnreferencedEntry()
    {
        var service = new MasterDataService(ContextFactory);
        var id = await service.CreateObservationAsync(new ObservationDetailDto { Description = "Temp" });
        await service.DeleteObservationAsync(id);

        var list = await service.GetObservationsAsync();
        Assert.That(list.Any(o => o.Id == id), Is.False);
    }
}
