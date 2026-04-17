using Microsoft.Extensions.Logging;
using NSubstitute;
using SpotAnalysis.Data.Enums;
using SpotAnalysis.Services.Services;

namespace SpotAnalysis.Services.Tests;

public class TestAdminService : BaseDatabaseTest
{
    private AdminService _adminService;
    private IUserService _userService;
    
    [OneTimeSetUp]
    public void InitAdminService()
    {
        var logger = Substitute.For<ILogger<AdminService>>();
        _adminService = new AdminService(ContextFactory, logger);
        _userService = new UserService(ContextFactory);
    }

    [Test]
    public async Task AddRoleToUser_RemoveRoleFromUser()
    {
        await CleanUpDb();
        
        await _userService.Register("RandomUser", "RandomPassword");
        
        var user = await _userService.Login("RandomUser", "RandomPassword");
        
        Assert.That(user, Is.Not.Null);
        Assert.That(user.Roles, Has.Count.EqualTo(1));
        Assert.That(user.Roles, Contains.Item(Role.Student));

        await _adminService.AddRoleToUser(user.UserID, Role.Teacher);
        
        user = await _userService.Login("RandomUser", "RandomPassword");
        
        Assert.That(user, Is.Not.Null);
        Assert.That(user.Roles, Has.Count.EqualTo(2));
        Assert.That(user.Roles, Contains.Item(Role.Teacher));
        Assert.That(user.Roles, Contains.Item(Role.Student));
        
        await _adminService.RemoveRoleFromUser(user.UserID, Role.Teacher);
        
        user = await _userService.Login("RandomUser", "RandomPassword");
        Assert.That(user, Is.Not.Null);
        Assert.That(user.Roles, Has.Count.EqualTo(1));
        Assert.That(user.Roles, Contains.Item(Role.Student));
    }

    [Test]
    public async Task DeleteUser()
    {
        await CleanUpDb();
        
        await _userService.Register("RandomUser", "RandomPassword");
        
        var user = await _userService.Login("RandomUser", "RandomPassword");
        
        
        Assert.That(user, Is.Not.Null);
        
        await _adminService.DeleteUser(user.UserID);
        user = await _userService.Login("RandomUser", "RandomPassword");
        Assert.That(user, Is.Null);
    }
}