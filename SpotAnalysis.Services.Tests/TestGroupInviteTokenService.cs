using Microsoft.EntityFrameworkCore;
using SpotAnalysis.Data.Models.Identity;
using SpotAnalysis.Services.Services;

namespace SpotAnalysis.Services.Tests;

public class TestGroupInviteTokenService : BaseDatabaseTest
{
    private IGroupInviteTokenService _svc = default!;

    private const int SeededGroupId = 1;
    private const string SeededGroupName = "Invite Test Group";

    [OneTimeSetUp]
    public async Task InitInviteTokenService()
    {
        _svc = new GroupInviteTokenService(ContextFactory);

        await using var ctx = await ContextFactory.CreateDbContextAsync();
        if (!await ctx.Groups.AnyAsync(g => g.GroupID == SeededGroupId))
        {
            ctx.Groups.Add(new Group
            {
                GroupID = SeededGroupId,
                Name = SeededGroupName,
            });
            await ctx.SaveChangesAsync();
        }
    }

    [SetUp]
    public async Task WipeInvites()
    {
        await using var ctx = await ContextFactory.CreateDbContextAsync();
        await ctx.GroupInvites.ExecuteDeleteAsync();
    }

    [Test]
    public async Task CreateToken_ThenValidate_ReturnsGroupId()
    {
        var token = await _svc.CreateToken(SeededGroupId);
        var result = await _svc.ValidateToken(token);

        Assert.That(result, Is.EqualTo(SeededGroupId));
    }

    [Test]
    public async Task CreateToken_ProducesSixCharCode()
    {
        var token = await _svc.CreateToken(SeededGroupId);

        Assert.That(token, Has.Length.EqualTo(6));
    }

    [Test]
    public async Task ValidateToken_IsCaseInsensitive()
    {
        var token = await _svc.CreateToken(SeededGroupId);
        var result = await _svc.ValidateToken(token.ToLowerInvariant());

        Assert.That(result, Is.EqualTo(SeededGroupId));
    }

    [Test]
    public async Task ValidateToken_TrimsWhitespace()
    {
        var token = await _svc.CreateToken(SeededGroupId);
        var result = await _svc.ValidateToken($"  {token}  ");

        Assert.That(result, Is.EqualTo(SeededGroupId));
    }

    [Test]
    public async Task ValidateToken_ReturnsNull_ForUnknownCode()
    {
        var result = await _svc.ValidateToken("ZZZZZZ");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task ValidateToken_ReturnsNull_ForEmpty()
    {
        Assert.That(await _svc.ValidateToken(""), Is.Null);
        Assert.That(await _svc.ValidateToken("   "), Is.Null);
    }

    [Test]
    public async Task ValidateToken_ReturnsNull_ForExpiredCode()
    {
        var token = await _svc.CreateToken(SeededGroupId);

        await using (var ctx = await ContextFactory.CreateDbContextAsync())
        {
            var invite = await ctx.GroupInvites.SingleAsync(i => i.Code == token);
            invite.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);
            await ctx.SaveChangesAsync();
        }

        var result = await _svc.ValidateToken(token);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CreateToken_TwiceForSameGroup_ProducesTwoUsableCodes()
    {
        var token1 = await _svc.CreateToken(SeededGroupId);
        var token2 = await _svc.CreateToken(SeededGroupId);

        Assert.Multiple(() =>
        {
            Assert.That(token1, Is.Not.EqualTo(token2));
        });

        Assert.That(await _svc.ValidateToken(token1), Is.EqualTo(SeededGroupId));
        Assert.That(await _svc.ValidateToken(token2), Is.EqualTo(SeededGroupId));
    }
}
