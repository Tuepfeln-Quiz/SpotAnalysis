using Microsoft.EntityFrameworkCore;
using SpotAnalysis.Services.DTOs;
using SpotAnalysis.Services.Services;

namespace SpotAnalysis.Services.Tests;

[TestFixture]
public class TestMasterDataServiceReactions : BaseDatabaseTest
{
    [SetUp]
    public async Task Reset() => await CleanUpDb();

    // ── Read ────────────────────────────────────────────────────────

    [Test]
    public async Task GetReactionsAsync_ReturnsAllWithChemicalNames()
    {
        var service = new MasterDataService(ContextFactory);

        var result = await service.GetReactionsAsync();

        Assert.That(result, Is.Not.Empty);
        Assert.That(result, Has.All.Matches<ReactionDetailDto>(r =>
            !string.IsNullOrEmpty(r.Chemical1Name) &&
            !string.IsNullOrEmpty(r.Chemical2Name)));
    }

    [Test]
    public async Task GetReactionByIdAsync_ReturnsNullForMissing()
    {
        var service = new MasterDataService(ContextFactory);

        var result = await service.GetReactionByIdAsync(999999);

        Assert.That(result, Is.Null);
    }

    // ── Create ──────────────────────────────────────────────────────

    [Test]
    public async Task CreateReactionAsync_NormalizesChemicalOrder()
    {
        var service = new MasterDataService(ContextFactory);
        await using var context = await ContextFactory.CreateDbContextAsync();
        var c1 = await context.Chemicals.OrderBy(c => c.ChemicalId).FirstAsync();
        var c2 = await context.Chemicals.OrderBy(c => c.ChemicalId).Skip(1).FirstAsync();
        var obs = await context.Observations.FirstAsync();

        var id = await service.CreateReactionAsync(new ReactionDetailDto
        {
            Chemical1Id = c2.ChemicalId, // bewusst umgekehrt
            Chemical2Id = c1.ChemicalId,
            RelevantProduct = "TestProdukt",
            Formula = "T",
            ObservationId = obs.ObservationId
        });

        var reaction = await context.Reactions.AsNoTracking().FirstAsync(r => r.ReactionId == id);
        Assert.That(reaction.Chemical1Id, Is.LessThanOrEqualTo(reaction.Chemical2Id));
    }

    [Test]
    public async Task CreateReactionAsync_Throws_OnDuplicateCombination()
    {
        var service = new MasterDataService(ContextFactory);
        await using var context = await ContextFactory.CreateDbContextAsync();
        var existing = await context.Reactions.AsNoTracking().FirstAsync();

        var dto = new ReactionDetailDto
        {
            Chemical1Id = existing.Chemical2Id, // umgekehrt
            Chemical2Id = existing.Chemical1Id,
            RelevantProduct = "X",
            Formula = "X",
            ObservationId = existing.ObservationId
        };

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.CreateReactionAsync(dto));
    }

    [Test]
    public async Task CreateReactionAsync_CreatesInlineObservation_WhenNewDescription()
    {
        var service = new MasterDataService(ContextFactory);
        await using var context = await ContextFactory.CreateDbContextAsync();
        var c1 = await context.Chemicals.OrderBy(c => c.ChemicalId).FirstAsync();
        var c2 = await context.Chemicals.OrderByDescending(c => c.ChemicalId).FirstAsync();

        var id = await service.CreateReactionAsync(new ReactionDetailDto
        {
            Chemical1Id = c1.ChemicalId,
            Chemical2Id = c2.ChemicalId,
            RelevantProduct = "Neu",
            Formula = "N",
            NewObservationDescription = "Brandneue Beobachtung"
        });

        var result = await service.GetReactionByIdAsync(id);
        Assert.That(result!.ObservationDescription, Is.EqualTo("Brandneue Beobachtung"));
    }

    // ── Update ──────────────────────────────────────────────────────

    [Test]
    public async Task UpdateReactionAsync_PersistsChanges_AndNormalizesOrder()
    {
        var service = new MasterDataService(ContextFactory);
        await using var context = await ContextFactory.CreateDbContextAsync();
        var existing = await context.Reactions.AsNoTracking().FirstAsync();

        var dto = await service.GetReactionByIdAsync(existing.ReactionId);
        dto!.RelevantProduct = "GeändertesProdukt";
        (dto.Chemical1Id, dto.Chemical2Id) = (dto.Chemical2Id, dto.Chemical1Id);

        await service.UpdateReactionAsync(dto);

        var reloaded = await context.Reactions.AsNoTracking().FirstAsync(r => r.ReactionId == existing.ReactionId);
        Assert.That(reloaded.RelevantProduct, Is.EqualTo("GeändertesProdukt"));
        Assert.That(reloaded.Chemical1Id, Is.LessThanOrEqualTo(reloaded.Chemical2Id));
    }

    // ── Delete ──────────────────────────────────────────────────────

    [Test]
    public async Task DeleteReactionAsync_RemovesEntry_WhenUnreferenced()
    {
        var service = new MasterDataService(ContextFactory);
        await using var context = await ContextFactory.CreateDbContextAsync();

        var c1 = await context.Chemicals.OrderBy(c => c.ChemicalId).FirstAsync();
        var c2 = await context.Chemicals.OrderBy(c => c.ChemicalId).Skip(2).FirstAsync();
        var obs = await context.Observations.FirstAsync();

        var id = await service.CreateReactionAsync(new ReactionDetailDto
        {
            Chemical1Id = c1.ChemicalId,
            Chemical2Id = c2.ChemicalId,
            RelevantProduct = "Temp",
            Formula = "T",
            ObservationId = obs.ObservationId
        });

        await service.DeleteReactionAsync(id);

        Assert.That(await service.GetReactionByIdAsync(id), Is.Null);
    }
}
