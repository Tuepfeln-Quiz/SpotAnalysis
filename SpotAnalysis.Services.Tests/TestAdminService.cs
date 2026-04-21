using Microsoft.Extensions.Logging;
using NSubstitute;
using SpotAnalysis.Data.Enums;
using SpotAnalysis.Services.Services;
using System.Linq;

namespace SpotAnalysis.Services.Tests;

public class TestAdminService : BaseDatabaseTest
{
    private IAdminService _adminService;
    private IUserService _userService;

    [OneTimeSetUp]
    public void InitServices()
    {
        var adminLogger = Substitute.For<ILogger<AdminService>>();
        var userLogger = Substitute.For<ILogger<UserService>>();
        _adminService = new AdminService(ContextFactory, adminLogger);
        _userService = new UserService(userLogger, ContextFactory);
    }

    [Test]
    public async Task AddRoleToUser_RemoveRoleFromUser()
    {
        await CleanUpDb();

        await _userService.Register("RandomUser", "RandomPassword12!");

        var user = await _userService.Login("RandomUser", "RandomPassword12!");

        Assert.That(user, Is.Not.Null);
        Assert.That(user.Roles, Has.Count.EqualTo(1));
        Assert.That(user.Roles, Contains.Item(Role.Student));

        await _adminService.AddRoleToUser(user.UserID, Role.Teacher);

        user = await _userService.Login("RandomUser", "RandomPassword12!");

        Assert.That(user, Is.Not.Null);
        Assert.That(user.Roles, Has.Count.EqualTo(2));
        Assert.That(user.Roles, Contains.Item(Role.Teacher));
        Assert.That(user.Roles, Contains.Item(Role.Student));

        await _adminService.RemoveRoleFromUser(user.UserID, Role.Teacher);

        user = await _userService.Login("RandomUser", "RandomPassword12!");
        Assert.That(user, Is.Not.Null);
        Assert.That(user.Roles, Has.Count.EqualTo(1));
        Assert.That(user.Roles, Contains.Item(Role.Student));
    }

    [Test]
    public async Task DeleteUser()
    {
        await CleanUpDb();

        await _userService.Register("RandomUser", "RandomPassword12!");

        var user = await _userService.Login("RandomUser", "RandomPassword12!");

        Assert.That(user, Is.Not.Null);

        await _adminService.DeleteUser(user.UserID);
        Assert.ThrowsAsync<ArgumentException>(async () =>
            await _userService.Login("RandomUser", "RandomPassword12!"));
    }

    [Test]
    public async Task GetUsersByRole()
    {
        await CleanUpDb();

        await _userService.Register("RandomUser", "RandomPassword12!");
        var user = await _userService.Login("RandomUser", "RandomPassword12!");
        Assert.That(user, Is.Not.Null);

        var students = await _adminService.GetUsersByRole(Role.Student);
        Assert.That(students, Is.Not.Empty);
        Assert.That(students.All(x => x.Roles.Contains(Role.Student.ToString())), Is.True);

        await _adminService.AddRoleToUser(user.UserID, Role.Admin);
        user = await _userService.Login("RandomUser", "RandomPassword12!");
        Assert.That(user, Is.Not.Null);
        Assert.That(user.Roles, Contains.Item(Role.Admin));
        var admins = await _adminService.GetUsersByRole(Role.Admin);
        Assert.That(admins, Is.Not.Empty);
        Assert.That(admins.All(x => x.Roles.Contains(Role.Admin.ToString())), Is.True);

        await _adminService.AddRoleToUser(user.UserID, Role.Teacher);
        user = await _userService.Login("RandomUser", "RandomPassword12!");
        Assert.That(user, Is.Not.Null);
        Assert.That(user.Roles, Contains.Item(Role.Teacher));
        var teachers = await _adminService.GetUsersByRole(Role.Teacher);
        Assert.That(teachers, Is.Not.Empty);
        Assert.That(teachers.All(x => x.Roles.Contains(Role.Teacher.ToString())), Is.True);
    }
}