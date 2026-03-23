using SpotAnalysis.Services.DTOs;
using SpotAnalysis.Services.Services;

namespace SpotAnalysis.Services.Tests;

public class TestTeacherService : BaseDatabaseTest
{
    protected ITeacherService TeacherService;

    [OneTimeSetUp]
    public void InitTeacherService()
    {
        TeacherService = new TeacherService(ContextFactory);
    }
    
    [Test]
    public async Task TestTeacherCreateGroup()
    {
        await TeacherService.CreateGroup(1, new ConfigGroupDto
        {
            Name = "Test Group",
            Description = "Test description"
        });

        var groups = await TeacherService.GetGroups(1);
        
        Assert.That(groups.Any(g => g.Name == "Test Group"));
    }
}