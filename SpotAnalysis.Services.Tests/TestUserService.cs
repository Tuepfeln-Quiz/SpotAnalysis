using System.Security.Authentication;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SpotAnalysis.Data.Enums;
using SpotAnalysis.Services.Services;

namespace SpotAnalysis.Services.Tests;

public class TestUserService : BaseDatabaseTest
{
#pragma warning disable CA1859
    private IUserService _userService;
#pragma warning restore CA1859

    #region Users

    private static readonly Guid Teacher3 = Guid.Parse("4c1ff247-7593-4839-ba32-1ae4867dd48c");
    private static readonly Guid Teacher4 = Guid.Parse("52d46405-a25b-48c0-8430-06ecb41fa879");
    private static readonly Guid Student3 = Guid.Parse("2a15e984-5c8f-4ff5-b344-3738e384ec31");
    private static readonly Guid Student4 = Guid.Parse("c4dca57f-5747-45b5-a4c3-af68be2a33fa");

    private const string StudentName3 = "Student 3";
    private const string StudentName4 = "Student 4";

    private const string StudentPassword3 = "password";

    #endregion
    
    #region helpers
    
    private static Random random = new Random();
    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
    
    #endregion
    
    [OneTimeSetUp]
    public void InitStudentService()
    {
        var logger = Substitute.For<ILogger<UserService>>();
        _userService = new UserService(logger, ContextFactory);
    }

    [Test]
    public async Task TestAllUserService()
    {
        var registeredUsers = new HashSet<(string, string)>();
        
        #region TestStudentRegister

        {
            await _userService.Register(StudentName3, StudentPassword3, null, Student3);

            for (var i = 0; i < 100; i++)
            {
                var uname = RandomString(12);
                var password = RandomString(12);
                registeredUsers.Add((uname, password));
                await _userService.Register(uname, password);
            }
        }

        #endregion
        
        #region TestStudentLogin

        {
            var user = await _userService.Login(StudentName3, StudentPassword3);
            Assert.That(user, Is.Not.Null);
            Assert.That(user.Roles.Contains(Role.Student));

            foreach (var (uname, password) in registeredUsers)
            {
                user = await _userService.Login(uname, password);
                Assert.That(user, Is.Not.Null);
            }
        }
        
        #endregion
        
        #region TestStudentFailLogin

        {
            var user = await _userService.Login(StudentName3, "AWrongPassword");
            Assert.That(user, Is.Null);

            foreach (var (uname, _) in registeredUsers)
            {
                user  = await _userService.Login(uname, RandomString(12));
                Assert.That(user, Is.Null);
            }
        }
        
        #endregion
    }
}