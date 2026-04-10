using SpotAnalysis.Services.DTOs;
using SpotAnalysis.Services.Services;

namespace SpotAnalysis.Services.Tests;

public class TestTeacherService : BaseDatabaseTest
{
#pragma warning disable CA1859
    private ITeacherService _teacherService;
#pragma warning restore CA1859

    #region Users

    private static readonly Guid Teacher1 = Guid.Parse("9c9c2138-f945-41fa-823e-f3bd286c0fa1");
    private static readonly Guid Teacher2 = Guid.Parse("48bb93c8-214f-47f0-910f-9056b19de94a");
    private static readonly Guid Student1 = Guid.Parse("2195c82c-0a67-4938-9c88-20c089276da5");
    private static readonly Guid Student2 = Guid.Parse("f01c1e4f-c5e0-4f77-a3b3-f59f8b837553");

    #endregion

    #region Groups

    private const int GroupId1 = 1;
    private const string GroupName1 = "Test Group";
    private const string GroupDescription1 = "Test description";

    #endregion

    [OneTimeSetUp]
    public void InitTeacherService()
    {
        _teacherService = new TeacherService(ContextFactory);
    }

    [Test]
    public async Task TestAllTeacherService()
    {
        #region TestCreateGroup

        {
            await _teacherService.CreateGroup(Teacher1, new ConfigGroupDto
            {
                Name = GroupName1,
                Description = GroupDescription1
            });

            var groups = await _teacherService.GetGroups(Teacher1);
        
            Assert.That(groups, Has.Count.EqualTo(1));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(groups[0].Name, Is.EqualTo(GroupName1));
                Assert.That(groups[0].Description, Is.EqualTo(GroupDescription1));
            }
        }

        #endregion

        #region TestGetStudents

        {
            var students = await _teacherService.GetStudents(Teacher1);
        
            Assert.That(students, Is.Empty);
        }

        #endregion

        #region TestAssignStudent

        {
            await _teacherService.AssignUserToGroup(Teacher1, Student1, GroupId1);
        
            var students = await _teacherService.GetStudents(Teacher1);
        
            Assert.That(students, Has.Count.EqualTo(1));
            Assert.That(students[0].Id, Is.EqualTo(Student1));
        }

        #endregion

        #region TestFailStudentAssign

        try
        {
            await _teacherService.AssignUserToGroup(Student1, Student2, GroupId1);

            Assert.Fail("Should have thrown an InvalidOperationException");
        }
        catch (InvalidOperationException)
        {

        }
        catch (Exception)
        {
            Assert.Fail("Should have thrown an InvalidOperationException");
        }

        #endregion

        #region TestGetStudentsByGroup

        {
            var students = await _teacherService.GetStudentsByGroup(Teacher1, GroupId1);
        
            Assert.That(students, Has.Count.EqualTo(1));
        }

        #endregion
        
        #region TestUpdateGroup

        {
            await _teacherService.UpdateGroup(Teacher1, new ConfigGroupDto
            {
                Name = GroupName1,
                Description = GroupDescription1 + "_edited"
            });
        
            var groups = await _teacherService.GetGroups(Teacher1);
        
            Assert.That(groups, Has.Count.EqualTo(1));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(groups[0].Name, Is.EqualTo(GroupName1));
                Assert.That(groups[0].Description, Is.EqualTo(GroupDescription1 + "_edited"));
            }
        }
        
        #endregion

        #region TestRemoveUserFromGroup

        {
            await _teacherService.RemoveUserFromGroup(Teacher1, Student1, GroupId1);
            var students = await _teacherService.GetStudentsByGroup(Teacher1, GroupId1);
            Assert.That(students, Has.Count.EqualTo(0));
        }

        #endregion

        #region TestDeleteGroup

        {
            await _teacherService.DeleteGroup(Teacher1, GroupId1);
            var groups = await _teacherService.GetGroups(Teacher1);
            Assert.That(groups, Has.Count.EqualTo(0));
        }

        #endregion
    }
}