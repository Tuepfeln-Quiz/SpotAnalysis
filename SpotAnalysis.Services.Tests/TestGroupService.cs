using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SpotAnalysis.Services.DTOs;
using SpotAnalysis.Services.Services;

namespace SpotAnalysis.Services.Tests;

public class TestGroupService : BaseDatabaseTest
{
    private IGroupService _groupService = default!;
    private IGroupInviteTokenService _inviteTokens = default!;

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

    private int _quizCountBeforeDelete;
    private int _userCountBeforeDelete;

    [OneTimeSetUp]
    public void InitGroupService()
    {
        var dpServices = new ServiceCollection();
        dpServices.AddDataProtection();
        var dpProvider = dpServices.BuildServiceProvider()
            .GetRequiredService<IDataProtectionProvider>();
        _inviteTokens = new GroupInviteTokenService(dpProvider);
        _groupService = new GroupService(ContextFactory, _inviteTokens);
    }

    [Test]
    public async Task TestAllGroupService()
    {
        #region TestCreateGroup

        {
            await _groupService.CreateGroup(Teacher1, new ConfigGroupDto
            {
                Name = GroupName1,
                Description = GroupDescription1
            });

            var groups = await _groupService.GetGroups(Teacher1);

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
            var students = await _groupService.GetStudents(Teacher1);

            Assert.That(students, Is.Empty);
        }

        #endregion

        #region TestAssignStudent

        {
            await _groupService.AssignUserToGroup(Teacher1, Student1, GroupId1);

            var students = await _groupService.GetStudents(Teacher1);

            Assert.That(students, Has.Count.EqualTo(1));
            Assert.That(students[0].Id, Is.EqualTo(Student1));
        }

        #endregion

        #region TestFailStudentAssign

        try
        {
            await _groupService.AssignUserToGroup(Student1, Student2, GroupId1);

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
            var students = await _groupService.GetStudentsByGroup(Teacher1, GroupId1);

            Assert.That(students, Has.Count.EqualTo(1));
        }

        #endregion

        #region TestUpdateGroup

        {
            await _groupService.UpdateGroup(Teacher1, new ConfigGroupDto
            {
                Name = GroupName1,
                Description = GroupDescription1 + "_edited"
            });

            var groups = await _groupService.GetGroups(Teacher1);

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
            await _groupService.RemoveUserFromGroup(Teacher1, Student1, GroupId1);
            var students = await _groupService.GetStudentsByGroup(Teacher1, GroupId1);
            Assert.That(students, Has.Count.EqualTo(0));
        }

        #endregion

        #region TestDeleteGroup

        {
            await _groupService.AssignUserToGroup(Teacher1, Student1, GroupId1);

            await using (var ctxBefore = await ContextFactory.CreateDbContextAsync())
            {
                _quizCountBeforeDelete = await ctxBefore.Quizzes.CountAsync();
                _userCountBeforeDelete = await ctxBefore.Users.CountAsync();
            }

            await _groupService.DeleteGroup(Teacher1, GroupId1);

            var groups = await _groupService.GetGroups(Teacher1);
            Assert.That(groups, Has.Count.EqualTo(0));

            await using (var ctxAfter = await ContextFactory.CreateDbContextAsync())
            {
                var quizCountAfter = await ctxAfter.Quizzes.CountAsync();
                var userCountAfter = await ctxAfter.Users.CountAsync();
                var groupStillExists = await ctxAfter.Groups.AnyAsync(g => g.GroupID == GroupId1);

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(groupStillExists, Is.False, "Gruppe sollte hard-deleted sein");
                    Assert.That(quizCountAfter, Is.EqualTo(_quizCountBeforeDelete), "Quizze dürfen nicht gelöscht werden");
                    Assert.That(userCountAfter, Is.EqualTo(_userCountBeforeDelete), "User dürfen nicht gelöscht werden");
                }
            }
        }

        #endregion
    }

    [Test]
    public async Task TestJoinGroupByToken()
    {
        const string groupName = "JoinTestGruppe";
        await _groupService.CreateGroup(Teacher2, new ConfigGroupDto
        {
            Name = groupName,
            Description = "for JoinGroupByToken test"
        });
        var groups = await _groupService.GetGroups(Teacher2);
        var groupId = groups.Single(g => g.Name == groupName).Id;

        #region Success

        var token = _inviteTokens.CreateToken(groupId);
        var result = await _groupService.JoinGroupByToken(Student2, token);
        Assert.That(result, Is.EqualTo(JoinGroupResult.Success));

        var members = await _groupService.GetStudentsByGroup(Teacher2, groupId);
        Assert.That(members.Any(m => m.Id == Student2), Is.True);

        #endregion

        #region AlreadyMember

        var token2 = _inviteTokens.CreateToken(groupId);
        var result2 = await _groupService.JoinGroupByToken(Student2, token2);
        Assert.That(result2, Is.EqualTo(JoinGroupResult.AlreadyMember));

        #endregion

        #region TokenInvalid

        var resultInvalid = await _groupService.JoinGroupByToken(Student1, "garbage");
        Assert.That(resultInvalid, Is.EqualTo(JoinGroupResult.TokenInvalid));

        #endregion

        #region GroupNotFound

        var tokenForGhost = _inviteTokens.CreateToken(99999);
        var resultGhost = await _groupService.JoinGroupByToken(Student1, tokenForGhost);
        Assert.That(resultGhost, Is.EqualTo(JoinGroupResult.GroupNotFound));

        #endregion

        #region UserNotFound

        var tokenForGroup = _inviteTokens.CreateToken(groupId);
        var resultNoUser = await _groupService.JoinGroupByToken(Guid.NewGuid(), tokenForGroup);
        Assert.That(resultNoUser, Is.EqualTo(JoinGroupResult.UserNotFound));

        #endregion

        await _groupService.DeleteGroup(Teacher2, groupId);
    }

    [Test]
    public async Task TestMultipleTeachersPerGroup()
    {
        const string groupName = "MultiTeacherGruppe";
        await _groupService.CreateGroup(Teacher1, new ConfigGroupDto
        {
            Name = groupName,
            Description = "Multi-Teacher test"
        });
        var groups = await _groupService.GetGroups(Teacher1);
        var groupId = groups.Single(g => g.Name == groupName).Id;

        // Teacher1 ist bereits Mitglied (automatisch bei CreateGroup)
        var teachers = await _groupService.GetTeachersByGroup(Teacher1, groupId);
        Assert.That(teachers, Has.Count.EqualTo(1));
        Assert.That(teachers[0].Id, Is.EqualTo(Teacher1));

        // Teacher2 zur Gruppe hinzufügen
        await _groupService.AssignUserToGroup(Teacher1, Teacher2, groupId);

        // Beide Lehrer sehen die Gruppe
        var groupsT1 = await _groupService.GetGroups(Teacher1);
        var groupsT2 = await _groupService.GetGroups(Teacher2);
        Assert.That(groupsT1.Any(g => g.Id == groupId), Is.True);
        Assert.That(groupsT2.Any(g => g.Id == groupId), Is.True);

        // GetTeachersByGroup liefert beide
        teachers = await _groupService.GetTeachersByGroup(Teacher1, groupId);
        Assert.That(teachers, Has.Count.EqualTo(2));

        // Schüler hinzufügen — taucht nicht bei GetTeachersByGroup auf
        await _groupService.AssignUserToGroup(Teacher1, Student1, groupId);
        teachers = await _groupService.GetTeachersByGroup(Teacher1, groupId);
        Assert.That(teachers, Has.Count.EqualTo(2), "Student darf nicht als Teacher erscheinen");

        // GetStudentsByGroup liefert nur den Schüler, nicht die Lehrer
        var students = await _groupService.GetStudentsByGroup(Teacher1, groupId);
        Assert.That(students, Has.Count.EqualTo(1));
        Assert.That(students[0].Id, Is.EqualTo(Student1));

        // Teacher2 entfernen
        await _groupService.RemoveUserFromGroup(Teacher1, Teacher2, groupId);
        teachers = await _groupService.GetTeachersByGroup(Teacher1, groupId);
        Assert.That(teachers, Has.Count.EqualTo(1));

        // Aufräumen
        await _groupService.DeleteGroup(Teacher1, groupId);
    }
}
