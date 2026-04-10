#define PASSWORD_PROVIDER_DETERMINISTIC

using SpotAnalysis.Services.Services;

namespace SpotAnalysis.Services.Tests;

public class TestStudentService : BaseDatabaseTest
{
#pragma warning disable CA1859
    private IStudentService _studentService;
    private ILoginService _loginService;
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
        _studentService = new StudentService(ContextFactory);
        _loginService = new LoginService(ContextFactory);
    }

    [Test]
    public async Task TestAllStudentService()
    {
        #region TestStudentRegister

        {
            await _studentService.Register(StudentName3, StudentPassword3, null, Student3);
        }

        #endregion
        
        #region TestStudentLogin

        {
            var user = await _loginService.Login(StudentName3, StudentPassword3);
            Assert.That(user, Is.Not.Null);
        }
        
        #endregion
        
        #region TestStudentFailLogin

        {
            var user = await _loginService.Login(StudentName3, "AWrongPassword");
            Assert.That(user, Is.Null);
        }
        
        #endregion
    }
}