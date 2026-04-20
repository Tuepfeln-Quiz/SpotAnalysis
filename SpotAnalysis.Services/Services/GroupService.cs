using Microsoft.EntityFrameworkCore;
using SpotAnalysis.Services.DTOs;
using SpotAnalysis.Data;
using SpotAnalysis.Data.Models.Identity;
using SpotAnalysis.Data.Enums;

namespace SpotAnalysis.Services.Services;

public class GroupService : IGroupService
{
    private readonly IDbContextFactory<AnalysisContext> _factory;
    private readonly IGroupInviteTokenService _inviteTokens;

    public GroupService(
        IDbContextFactory<AnalysisContext> factory,
        IGroupInviteTokenService inviteTokens)
    {
        _factory = factory;
        _inviteTokens = inviteTokens;
    }

    private static IQueryable<User> GetTeacher(AnalysisContext ctx, Guid teacherId)
    {
        return ctx.Users
            .Where(u => u.UserID == teacherId && u.Roles.Any(r => r == Role.Teacher));
    }

    #region Queries

    public async Task<List<GroupDto>> GetGroups(Guid teacherId)
    {
        await using var ctx = await _factory.CreateDbContextAsync();

        var teacher = GetTeacher(ctx, teacherId);

        return teacher.SelectMany(u => u.Groups)
            .Select(g => new GroupDto
            {
                Id = g.GroupID,
                Name = g.Name,
                Description = g.Description,
            }).ToList();
    }

    public async Task<List<StudentDto>> GetStudents(Guid teacherId)
    {
        await using var ctx = await _factory.CreateDbContextAsync();

        var teacher = GetTeacher(ctx, teacherId);

        return teacher.SelectMany(u => u.Groups)
            .SelectMany(g => g.Users)
            .Distinct()
            .Where(u => u.UserID != teacherId)
            .Select(u => new StudentDto
            {
                Id = u.UserID,
                UserName = u.UserName,
                AssignedGroups = u.Groups.Select(g => new GroupDto
                {
                    Id = g.GroupID,
                    Name = g.Name,
                }).ToList()
            }).ToList();
    }

    public async Task<List<StudentDto>> GetStudentsByGroup(Guid teacherId, int groupId)
    {
        await using var ctx = await _factory.CreateDbContextAsync();

        var teacher = GetTeacher(ctx, teacherId);

        return teacher.SelectMany(u => u.Groups)
            .Where(g => g.GroupID == groupId)
            .SelectMany(g => g.Users)
            .Where(u => u.Roles.Any(r => r == Role.Student))
            .Select(u => new StudentDto
            {
                Id = u.UserID,
                UserName = u.UserName,
                AssignedGroups = u.Groups.Select(g => new GroupDto
                {
                    Id = g.GroupID,
                    Name = g.Name,
                }).ToList()
            }).ToList();
    }

    public async Task<List<StudentDto>> GetTeachersByGroup(Guid teacherId, int groupId)
    {
        await using var ctx = await _factory.CreateDbContextAsync();

        var teacher = GetTeacher(ctx, teacherId);

        return teacher.SelectMany(u => u.Groups)
            .Where(g => g.GroupID == groupId)
            .SelectMany(g => g.Users)
            .Where(u => u.Roles.Any(r => r == Role.Teacher))
            .Select(u => new StudentDto
            {
                Id = u.UserID,
                UserName = u.UserName,
                AssignedGroups = u.Groups.Select(g => new GroupDto
                {
                    Id = g.GroupID,
                    Name = g.Name,
                }).ToList()
            }).ToList();
    }

    public async Task<List<StudentDto>> SearchTeachers(string query)
    {
        await using var ctx = await _factory.CreateDbContextAsync();

        var q = ctx.Users
            .Where(u => u.Roles.Any(r => r == Role.Teacher));

        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(u => u.UserName.Contains(query));

        return await q
            .Select(u => new StudentDto
            {
                Id = u.UserID,
                UserName = u.UserName,
                AssignedGroups = u.Groups.Select(g => new GroupDto
                {
                    Id = g.GroupID,
                    Name = g.Name,
                }).ToList()
            }).ToListAsync();
    }

    public async Task<List<StudentDto>> SearchStudents(string query)
    {
        await using var ctx = await _factory.CreateDbContextAsync();

        var q = ctx.Users
            .Where(u => u.Roles.Any(r => r == Role.Student));

        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(u => u.UserName.Contains(query));

        return await q
            .Select(u => new StudentDto
            {
                Id = u.UserID,
                UserName = u.UserName,
                AssignedGroups = u.Groups.Select(g => new GroupDto
                {
                    Id = g.GroupID,
                    Name = g.Name,
                }).ToList()
            }).ToListAsync();
    }

    public async Task<List<QuizOverviewDto>> GetQuizzesByGroup(int groupId)
    {
        await using var ctx = await _factory.CreateDbContextAsync();

        return await ctx.Groups
            .Where(g => g.GroupID == groupId)
            .SelectMany(g => g.Quizzes)
            .Select(q => new QuizOverviewDto
            {
                Id = q.QuizID,
                Name = q.Name,
                STCount = q.QuizQuestions.Count(qq => qq.Question.Type == QuestionType.SpotTest),
                STLCount = q.QuizQuestions.Count(qq => qq.Question.Type == QuestionType.SpotTestLight),
                GroupCount = q.Groups.Count
            }).ToListAsync();
    }

    #endregion

    #region CRUD

    public async Task CreateGroup(Guid teacherId, ConfigGroupDto group)
    {
        await using var ctx = await _factory.CreateDbContextAsync();
        var user = await GetTeacher(ctx, teacherId).SingleAsync();

        var qGroup = new Group
        {
            Name = group.Name,
            Description = group.Description,
        };

        qGroup.Users.Add(user);
        ctx.Groups.Add(qGroup);

        await ctx.SaveChangesAsync();
    }

    public async Task UpdateGroup(Guid teacherId, ConfigGroupDto group)
    {
        await using var ctx = await _factory.CreateDbContextAsync();

        var teacher = GetTeacher(ctx, teacherId);

        var qGroup = await teacher.SelectMany(u => u.Groups).SingleAsync(g => g.Name == group.Name);

        qGroup.Description = group.Description;
        qGroup.Name = group.Name;

        await ctx.SaveChangesAsync();
    }

    public async Task DeleteGroup(Guid teacherId, int groupId)
    {
        await using var ctx = await _factory.CreateDbContextAsync();

        var teacherQuery = GetTeacher(ctx, teacherId);
        var group = await teacherQuery
            .SelectMany(u => u.Groups)
            .Include(g => g.Users)
            .Include(g => g.Quizzes)
            .SingleAsync(g => g.GroupID == groupId);

        group.Users.Clear();
        group.Quizzes.Clear();
        ctx.Groups.Remove(group);

        await ctx.SaveChangesAsync();
    }

    #endregion

    #region Membership (teacher-authorized)

    public async Task AssignUserToGroup(Guid teacherId, Guid userId, int groupId)
    {
        await using var ctx = await _factory.CreateDbContextAsync();
        var user = await ctx.Users.SingleAsync(u => u.UserID == userId);
        var teacher = GetTeacher(ctx, teacherId);
        var group = await teacher.SelectMany(u => u.Groups).SingleAsync(g => g.GroupID == groupId);
        group.Users.Add(user);

        await ctx.SaveChangesAsync();
    }

    public async Task RemoveUserFromGroup(Guid teacherId, Guid userId, int groupId)
    {
        await using var ctx = await _factory.CreateDbContextAsync();

        var user = await ctx.Users.SingleAsync(u => u.UserID == userId);
        var teacher = GetTeacher(ctx, teacherId);
        var group = await teacher
            .SelectMany(u => u.Groups)
            .Include(u => u.Users)
            .SingleAsync(g => g.GroupID == groupId);

        group.Users.Remove(user);

        await ctx.SaveChangesAsync();
    }

    #endregion

    #region Membership (admin — no teacher-auth check)

    public async Task AssignUserToGroup(Guid userId, int groupId)
    {
        await using var ctx = await _factory.CreateDbContextAsync();

        var group = await ctx.Groups.SingleAsync(g => g.GroupID == groupId);
        var user = await ctx.Users.SingleAsync(u => u.UserID == userId);

        if (!user.Groups.Contains(group))
        {
            user.Groups.Add(group);
            await ctx.SaveChangesAsync();
        }
    }

    public async Task RemoveUserFromGroup(Guid userId, int groupId)
    {
        await using var ctx = await _factory.CreateDbContextAsync();

        var group = await ctx.Groups.SingleAsync(g => g.GroupID == groupId);
        var user = await ctx.Users.SingleAsync(u => u.UserID == userId);

        user.Groups.Remove(group);

        await ctx.SaveChangesAsync();
    }

    #endregion

    #region Token-based join

    public async Task<JoinGroupResult> JoinGroupByToken(Guid userId, string token)
    {
        var groupId = _inviteTokens.ValidateToken(token);
        if (groupId is null)
        {
            return JoinGroupResult.TokenInvalid;
        }

        await using var ctx = await _factory.CreateDbContextAsync();

        var group = await ctx.Groups
            .Include(g => g.Users)
            .SingleOrDefaultAsync(g => g.GroupID == groupId.Value);
        if (group is null)
        {
            return JoinGroupResult.GroupNotFound;
        }

        var user = await ctx.Users.SingleOrDefaultAsync(u => u.UserID == userId);
        if (user is null)
        {
            return JoinGroupResult.UserNotFound;
        }

        if (group.Users.Any(u => u.UserID == userId))
        {
            return JoinGroupResult.AlreadyMember;
        }

        group.Users.Add(user);
        await ctx.SaveChangesAsync();
        return JoinGroupResult.Success;
    }

    #endregion
}
