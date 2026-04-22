using Microsoft.EntityFrameworkCore;
using SpotAnalysis.Data.Enums;
using SpotAnalysis.Services.DTOs;
using SpotAnalysis.Services.Services;

namespace SpotAnalysis.Services.Tests;

[TestFixture]
public class TestMasterDataServiceChemicals : BaseDatabaseTest
{
    [SetUp]
    public async Task Reset() => await CleanUpDb();

    // ── Read ────────────────────────────────────────────────────────

    [Test]
    public async Task GetChemicalsAsync_ReturnsAllChemicalsWithMethodOutputs()
    {
        var service = new MasterDataService(ContextFactory);

        var result = await service.GetChemicalsAsync();

        Assert.That(result, Is.Not.Empty, "Seeded DB enthält Chemicals");
        var feCl3 = result.FirstOrDefault(c => c.Name == "Eisen(III)chlorid");
        Assert.That(feCl3, Is.Not.Null);
        Assert.That(feCl3!.Type, Is.EqualTo(ChemicalType.Educt));
        Assert.That(feCl3.Color, Is.EqualTo("orange"));
        Assert.That(feCl3.MethodOutputs.Any(mo => mo.MethodName == "ph-Papier" && mo.Color == "rot"), Is.True);
    }

    [Test]
    public async Task GetChemicalByIdAsync_ReturnsDto_WhenExists()
    {
        var service = new MasterDataService(ContextFactory);
        await using var context = await ContextFactory.CreateDbContextAsync();
        var existing = await context.Chemicals.FirstAsync(c => c.Name == "Eisen(III)chlorid");

        var result = await service.GetChemicalByIdAsync(existing.ChemicalID);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("Eisen(III)chlorid"));
    }

    [Test]
    public async Task GetChemicalByIdAsync_ReturnsNull_WhenMissing()
    {
        var service = new MasterDataService(ContextFactory);

        var result = await service.GetChemicalByIdAsync(999999);

        Assert.That(result, Is.Null);
    }

    // ── Create ──────────────────────────────────────────────────────

    [Test]
    public async Task CreateChemicalAsync_InsertsNewEntry()
    {
        var service = new MasterDataService(ContextFactory);
        var dto = new ChemicalDetailDto
        {
            Name = "Testsubstanz",
            Formula = "TS",
            Color = "grün",
            Type = ChemicalType.Educt,
            MethodOutputs = new()
            {
                new MethodOutputEntry { MethodName = "ph-Papier", Color = "gelb" }
            }
        };

        var id = await service.CreateChemicalAsync(dto);

        Assert.That(id, Is.GreaterThan(0));
        var result = await service.GetChemicalByIdAsync(id);
        Assert.That(result!.Name, Is.EqualTo("Testsubstanz"));
        Assert.That(result.MethodOutputs.Any(mo => mo.MethodName == "ph-Papier" && mo.Color == "gelb"), Is.True);
    }

    [Test]
    public void CreateChemicalAsync_Throws_OnDuplicateNameAndFormula()
    {
        var service = new MasterDataService(ContextFactory);
        var dto = new ChemicalDetailDto
        {
            Name = "Eisen(III)chlorid",
            Formula = "FeCl3",
            Color = "orange",
            Type = ChemicalType.Educt
        };

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.CreateChemicalAsync(dto));
    }

    // ── Update ──────────────────────────────────────────────────────

    [Test]
    public async Task UpdateChemicalAsync_PersistsChanges_IncludingMethodOutputs()
    {
        var service = new MasterDataService(ContextFactory);
        await using var context = await ContextFactory.CreateDbContextAsync();
        var existing = await context.Chemicals.FirstAsync(c => c.Name == "Eisen(III)chlorid");

        var dto = await service.GetChemicalByIdAsync(existing.ChemicalID);
        dto!.Color = "rotbraun";
        var phPaper = dto.MethodOutputs.First(mo => mo.MethodName == "ph-Papier");
        phPaper.Color = "hellrot";

        await service.UpdateChemicalAsync(dto);

        var reloaded = await service.GetChemicalByIdAsync(existing.ChemicalID);
        Assert.That(reloaded!.Color, Is.EqualTo("rotbraun"));
        Assert.That(reloaded.MethodOutputs.First(mo => mo.MethodName == "ph-Papier").Color,
            Is.EqualTo("hellrot"));
    }

    [Test]
    public async Task UpdateChemicalAsync_Throws_OnDuplicateNameAndFormula()
    {
        var service = new MasterDataService(ContextFactory);
        await using var context = await ContextFactory.CreateDbContextAsync();
        var a = await context.Chemicals.FirstAsync(c => c.Name == "Eisen(III)chlorid");
        var b = await context.Chemicals.Where(c => c.Name != "Eisen(III)chlorid").FirstAsync();

        var dto = await service.GetChemicalByIdAsync(b.ChemicalID);
        dto!.Name = a.Name;
        dto.Formula = a.Formula;

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.UpdateChemicalAsync(dto));
    }

    [Test]
    public async Task UpdateChemicalAsync_RemovesMethodOutput_WhenColorCleared()
    {
        var service = new MasterDataService(ContextFactory);
        await using var context = await ContextFactory.CreateDbContextAsync();
        var existing = await context.Chemicals.FirstAsync(c => c.Name == "Eisen(III)chlorid");

        var dto = await service.GetChemicalByIdAsync(existing.ChemicalID);
        var phPaper = dto!.MethodOutputs.First(mo => mo.MethodName == "ph-Papier");
        phPaper.Color = "";

        await service.UpdateChemicalAsync(dto);

        var reloaded = await service.GetChemicalByIdAsync(existing.ChemicalID);
        Assert.That(reloaded!.MethodOutputs.Any(mo => mo.MethodName == "ph-Papier"), Is.False);
    }

    // ── Delete ──────────────────────────────────────────────────────

    [Test]
    public async Task DeleteChemicalAsync_RemovesEntry_WhenUnreferenced()
    {
        var service = new MasterDataService(ContextFactory);
        var id = await service.CreateChemicalAsync(new ChemicalDetailDto
        {
            Name = "Löschkandidat",
            Formula = "LK",
            Color = "rosa",
            Type = ChemicalType.Additive
        });

        await service.DeleteChemicalAsync(id);

        var result = await service.GetChemicalByIdAsync(id);
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task DeleteChemicalAsync_Throws_WhenReferencedByReaction()
    {
        var service = new MasterDataService(ContextFactory);
        await using var context = await ContextFactory.CreateDbContextAsync();
        var chemical = await context.Chemicals
            .Include(c => c.Chemical1Reactions)
            .Include(c => c.Chemical2Reactions)
            .FirstAsync(c => c.Chemical1Reactions.Any() || c.Chemical2Reactions.Any());

        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.DeleteChemicalAsync(chemical.ChemicalID));

        Assert.That(ex!.Message, Does.Contain("referenziert"));
    }
}
