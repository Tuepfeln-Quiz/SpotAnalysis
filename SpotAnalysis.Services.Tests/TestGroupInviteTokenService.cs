using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using SpotAnalysis.Services.Services;

namespace SpotAnalysis.Services.Tests;

public class TestGroupInviteTokenService
{
    private static IGroupInviteTokenService CreateService()
    {
        var services = new ServiceCollection();
        services.AddDataProtection();
        var provider = services.BuildServiceProvider();
        return new GroupInviteTokenService(provider.GetRequiredService<IDataProtectionProvider>());
    }

    [Test]
    public void CreateToken_ThenValidate_ReturnsGroupId()
    {
        var svc = CreateService();

        var token = svc.CreateToken(42);
        var result = svc.ValidateToken(token);

        Assert.That(result, Is.EqualTo(42));
    }

    [Test]
    public void ValidateToken_ReturnsNull_ForTamperedToken()
    {
        var svc = CreateService();

        var token = svc.CreateToken(42);
        var tampered = token.Substring(0, token.Length - 1) + (token[^1] == 'a' ? 'b' : 'a');

        var result = svc.ValidateToken(tampered);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void ValidateToken_ReturnsNull_ForEmptyToken()
    {
        var svc = CreateService();
        Assert.That(svc.ValidateToken(""), Is.Null);
        Assert.That(svc.ValidateToken("   "), Is.Null);
    }

    [Test]
    public void ValidateToken_ReturnsNull_ForGarbageString()
    {
        var svc = CreateService();
        Assert.That(svc.ValidateToken("not-a-real-token"), Is.Null);
    }

}
