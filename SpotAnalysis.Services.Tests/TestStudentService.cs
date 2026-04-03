#define PASSWORD_PROVIDER_DETERMINISTIC

using SpotAnalysis.Services.Services;

namespace SpotAnalysis.Services.Tests;

public class TestStudentService : BaseDatabaseTest
{
#pragma warning disable CA1859
    private IStudentService _studentService;
#pragma warning restore CA1859
    
    [OneTimeSetUp]
    public void InitStudentService()
    {
        _studentService = new StudentService(ContextFactory);
    }

    [Test]
    public async Task TestAllStudentService()
    {
        
    }
}