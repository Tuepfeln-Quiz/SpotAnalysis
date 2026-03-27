using SpotAnalysis.Services.DTOs;
using SpotAnalysis.Services.Services;

namespace SpotAnalysis.Services.Tests;

public class TestTeacherService : BaseDatabaseTest
{
#pragma warning disable CA1859
    private ITeacherService _teacherService;
#pragma warning restore CA1859

    [OneTimeSetUp]
    public void InitTeacherService()
    {
        _teacherService = new TeacherService(ContextFactory);
    }
    
    [Test, Order(1)]
    public async Task TestTeacherCreateGroup()
    {
        await _teacherService.CreateGroup(1, new ConfigGroupDto
        {
            Name = "Test Group",
            Description = "Test description"
        });

        var groups = await _teacherService.GetGroups(1);
        
        Assert.That(groups, Has.Count.EqualTo(1));
        Assert.That(groups[0].Name, Is.EqualTo("Test Group"));
    }

    [Test, Order(2)]
    public async Task TestTeacherGetStudents()
    {
        var students = await _teacherService.GetStudents(1);
        
        Assert.That(students, Is.Empty);
    }

    [Test, Order(3)]
    public async Task TestTeacherAssignStudent()
    {
        await _teacherService.AssignUserToGroup(1, 3, 1);
        
        var students = await _teacherService.GetStudents(1);
        
        Assert.That(students, Has.Count.EqualTo(1));
    }
}