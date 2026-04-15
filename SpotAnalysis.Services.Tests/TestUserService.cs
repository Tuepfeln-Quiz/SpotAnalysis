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
    
    [OneTimeSetUp]
    public void InitStudentService()
    {
        _userService = new UserService(ContextFactory);
    }

    [Test]
    public async Task TestAllUserService()
    {
        #region TestStudentRegister

        {
            await _userService.Register(StudentName3, StudentPassword3, null, Student3);
        }

        #endregion
        
        #region TestStudentLogin

        {
            var user = await _userService.Login(StudentName3, StudentPassword3);
            Assert.That(user, Is.Not.Null);
        }
        
        #endregion
        
        #region TestStudentFailLogin

        {
            var user = await _userService.Login(StudentName3, "AWrongPassword");
            Assert.That(user, Is.Null);
        }
        
        #endregion
    }
}