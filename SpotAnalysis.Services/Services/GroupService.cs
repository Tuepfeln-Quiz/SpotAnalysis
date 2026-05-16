using Microsoft.EntityFrameworkCore;
using SpotAnalysis.Data;
using SpotAnalysis.Data.Enums;
using SpotAnalysis.Data.Models.Identity;
using SpotAnalysis.Services.DTOs;

namespace SpotAnalysis.Services.Services;

public class GroupService(
    IDbContextFactory<AnalysisContext> factory,
    IGroupInviteTokenService inviteTokens)
    : IGroupService
{
    #region Token-based join

    public async Task<JoinGroupResult> JoinGroupByToken(Guid userId, string token)
    {
        var invite = await inviteTokens.ValidateToken(token);
        if (invite is null)
            return JoinGroupResult.TokenInvalid;

        if (invite.ExpiresAt <= DateTime.UtcNow)
            return JoinGroupResult.TokenExpired;

        await using var ctx = await factory.CreateDbContextAsync();

        var group = await ctx.Groups
            .Include(g => g.Users)
            .SingleOrDefaultAsync(g => g.GroupId == invite.GroupId);
        if (group is null)
            return JoinGroupResult.GroupNotFound;

        var user = await ctx.Users.SingleOrDefaultAsync(u => u.UserId == userId);
        if (user is null)
        {
            return JoinGroupResult.UserNotFound;
        }

        if (group.Users.Any(u => u.UserId == userId))
        {
            return JoinGroupResult.AlreadyMember;
        }

        group.Users.Add(user);
        await ctx.SaveChangesAsync();
        return JoinGroupResult.Success;
    }

    #endregion

    // Admins see/manage all groups; teachers only the ones they are a member of.
    private static async Task<bool> IsAdmin(AnalysisContext ctx, Guid userId)
    {
        return await ctx.Users
            .AnyAsync(u => u.UserId == userId && u.Roles.Any(r => r == Role.Admin));
    }

    private static IQueryable<Group> AccessibleGroups(AnalysisContext ctx, Guid userId, bool isAdmin)
    {
        if (isAdmin)
            return ctx.Groups;
        return ctx.Users
            .Where(u => u.UserId == userId && u.Roles.Any(r => r == Role.Teacher))
            .SelectMany(u => u.Groups);
    }

    #region Queries

    public async Task<List<GroupDto>> GetGroups(Guid userId)
    {
        await using var ctx = await factory.CreateDbContextAsync();

        var isAdmin = await IsAdmin(ctx, userId);

        return await AccessibleGroups(ctx, userId, isAdmin)
            .Select(g => new GroupDto { Id = g.GroupId, Name = g.Name, Description = g.Description, }).ToListAsync();
    }

    public async Task<List<StudentDto>> GetStudents(Guid userId)
    {
        await using var ctx = await factory.CreateDbContextAsync();

        var isAdmin = await IsAdmin(ctx, userId);

        return await AccessibleGroups(ctx, userId, isAdmin)
            .SelectMany(g => g.Users)
            .Distinct()
            .Where(u => u.UserId != userId)
            .Select(u => new StudentDto
            {
                Id = u.UserId,
                UserName = u.UserName,
                AssignedGroups = u.Groups.Select(g => new GroupDto { Id = g.GroupId, Name = g.Name, }).ToList()
            }).ToListAsync();
    }

    public async Task<List<StudentDto>> GetStudentsByGroup(Guid userId, int groupId)
    {
        await using var ctx = await factory.CreateDbContextAsync();

        var isAdmin = await IsAdmin(ctx, userId);

        return await AccessibleGroups(ctx, userId, isAdmin)
            .Where(g => g.GroupId == groupId)
            .SelectMany(g => g.Users)
            .Where(u => u.Roles.Any(r => r == Role.Student)
                        && !u.Roles.Any(r => r == Role.Teacher || r == Role.Admin))
            .Select(u => new StudentDto
            {
                Id = u.UserId,
                UserName = u.UserName,
                AssignedGroups = u.Groups.Select(g => new GroupDto { Id = g.GroupId, Name = g.Name, }).ToList()
            }).ToListAsync();
    }

    public async Task<List<StudentDto>> GetTeachersByGroup(Guid userId, int groupId)
    {
        await using var ctx = await factory.CreateDbContextAsync();

        var isAdmin = await IsAdmin(ctx, userId);

        return await AccessibleGroups(ctx, userId, isAdmin)
            .Where(g => g.GroupId == groupId)
            .SelectMany(g => g.Users)
            .Where(u => u.Roles.Any(r => r == Role.Teacher))
            .Select(u => new StudentDto
            {
                Id = u.UserId,
                UserName = u.UserName,
                AssignedGroups = u.Groups.Select(g => new GroupDto { Id = g.GroupId, Name = g.Name, }).ToList()
            }).ToListAsync();
    }

    public async Task<List<StudentDto>> SearchTeachers(string query)
    {
        await using var ctx = await factory.CreateDbContextAsync();

        var q = ctx.Users
            .Where(u => u.Roles.Any(r => r == Role.Teacher));

        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(u => u.UserName.Contains(query));

        return await q
            .Select(u => new StudentDto
            {
                Id = u.UserId,
                UserName = u.UserName,
                AssignedGroups = u.Groups.Select(g => new GroupDto { Id = g.GroupId, Name = g.Name, }).ToList()
            }).ToListAsync();
    }

    public async Task<List<StudentDto>> SearchStudents(string query)
    {
        await using var ctx = await factory.CreateDbContextAsync();

        // Ein Lehrer hat konventionsgemaess auch die Student-Rolle; hier sollen
        // nur "echte" Schueler erscheinen, daher Teacher/Admin ausschliessen.
        var q = ctx.Users
            .Where(u => u.Roles.Any(r => r == Role.Student)
                        && !u.Roles.Any(r => r == Role.Teacher || r == Role.Admin));

        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(u => u.UserName.Contains(query));

        return await q
            .Select(u => new StudentDto
            {
                Id = u.UserId,
                UserName = u.UserName,
                AssignedGroups = u.Groups.Select(g => new GroupDto { Id = g.GroupId, Name = g.Name, }).ToList()
            }).ToListAsync();
    }

    public async Task<List<QuizOverviewDto>> GetQuizzesByGroup(int groupId)
    {
        await using var ctx = await factory.CreateDbContextAsync();

        return await ctx.Groups
            .Where(g => g.GroupId == groupId)
            .SelectMany(g => g.Quizzes)
            .Select(q => new QuizOverviewDto
            {
                Id = q.QuizId,
                Name = q.Name,
                STCount = q.QuizQuestions.Count(qq => qq.Question.Type == QuestionType.SpotTest),
                STLCount = q.QuizQuestions.Count(qq => qq.Question.Type == QuestionType.SpotTestLight),
                GroupCount = q.Groups.Count
            }).ToListAsync();
    }

    #endregion

    #region CRUD

    public async Task CreateGroup(Guid userId, ConfigGroupDto group)
    {
        await using var ctx = await factory.CreateDbContextAsync();

        var actor = await ctx.Users.SingleAsync(u => u.UserId == userId);
        var isTeacher = actor.Roles.Contains(Role.Teacher);
        var isAdmin = actor.Roles.Contains(Role.Admin);
        if (!isTeacher && !isAdmin)
            throw new UnauthorizedAccessException("User is neither teacher nor admin.");

        var qGroup = new Group { Name = group.Name, Description = group.Description, };

        // Only auto-add the creator as a member if they are a teacher.
        // Pure admins create groups without becoming a member (they see all groups anyway).
        if (isTeacher)
            qGroup.Users.Add(actor);

        ctx.Groups.Add(qGroup);

        await ctx.SaveChangesAsync();
    }

    public async Task UpdateGroup(Guid userId, ConfigGroupDto group)
    {
        await using var ctx = await factory.CreateDbContextAsync();

        var isAdmin = await IsAdmin(ctx, userId);

        var qGroup = await AccessibleGroups(ctx, userId, isAdmin)
            .SingleAsync(g => g.Name == group.Name);

        qGroup.Description = group.Description;
        qGroup.Name = group.Name;

        await ctx.SaveChangesAsync();
    }

    public async Task DeleteGroup(Guid userId, int groupId)
    {
        await using var ctx = await factory.CreateDbContextAsync();

        var isAdmin = await IsAdmin(ctx, userId);

        var group = await AccessibleGroups(ctx, userId, isAdmin)
            .Include(g => g.Users)
            .Include(g => g.Quizzes)
            .SingleAsync(g => g.GroupId == groupId);

        group.Users.Clear();
        group.Quizzes.Clear();
        ctx.Groups.Remove(group);

        await ctx.SaveChangesAsync();
    }

    #endregion

    #region Membership (teacher/admin-authorized)

    public async Task AssignUserToGroup(Guid actorId, Guid userId, int groupId)
    {
        await using var ctx = await factory.CreateDbContextAsync();

        var isAdmin = await IsAdmin(ctx, actorId);

        var user = await ctx.Users.SingleAsync(u => u.UserId == userId);
        var group = await AccessibleGroups(ctx, actorId, isAdmin)
            .SingleAsync(g => g.GroupId == groupId);

        group.Users.Add(user);

        await ctx.SaveChangesAsync();
    }

    public async Task RemoveUserFromGroup(Guid actorId, Guid userId, int groupId)
    {
        await using var ctx = await factory.CreateDbContextAsync();

        var isAdmin = await IsAdmin(ctx, actorId);

        var user = await ctx.Users.SingleAsync(u => u.UserId == userId);
        var group = await AccessibleGroups(ctx, actorId, isAdmin)
            .Include(g => g.Users)
            .SingleAsync(g => g.GroupId == groupId);

        group.Users.Remove(user);

        await ctx.SaveChangesAsync();
    }

    #endregion

    #region Membership (admin — no teacher-auth check)

    public async Task AssignUserToGroup(Guid userId, int groupId)
    {
        await using var ctx = await factory.CreateDbContextAsync();

        var group = await ctx.Groups.SingleAsync(g => g.GroupId == groupId);
        var user = await ctx.Users.Include(u => u.Groups).SingleAsync(u => u.UserId == userId);

        if (!user.Groups.Contains(group))
        {
            user.Groups.Add(group);
            await ctx.SaveChangesAsync();
        }
    }

    public async Task RemoveUserFromGroup(Guid userId, int groupId)
    {
        await using var ctx = await factory.CreateDbContextAsync();

        var group = await ctx.Groups.SingleAsync(g => g.GroupId == groupId);
        var user = await ctx.Users.SingleAsync(u => u.UserId == userId);

        user.Groups.Remove(group);

        await ctx.SaveChangesAsync();
    }

    #endregion
}
