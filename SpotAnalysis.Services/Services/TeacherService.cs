using Microsoft.EntityFrameworkCore;
using SpotAnalysis.Services.DTOs;
using SpotAnalysis.Data;
using SpotAnalysis.Data.Models.Identity;

namespace SpotAnalysis.Services.Services;

public class TeacherService(IDbContextFactory<AnalysisContext> factory) : ITeacherService
{
    private static IQueryable<User> GetTeacher(AnalysisContext ctx, Guid teacherId)
    {
        return ctx.Users
            .Where(u => u.UserID == teacherId && u.Roles.Any(r => r.Title == "teacher"));
    }
    // Tested
    public async Task<List<StudentDto>> GetStudents(Guid teacherId)
    {
        await using var ctx = await factory.CreateDbContextAsync();

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

    // Tested
    public async Task<List<StudentDto>> GetStudentsByGroup(Guid teacherId, int groupId)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        
        var teacher = GetTeacher(ctx, teacherId);

        return teacher.SelectMany(u => u.Groups)
            .Where(g => g.GroupID == groupId)
            .SelectMany(g => g.Users)
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

    // Tested
    public async Task<List<GroupDto>> GetGroups(Guid teacherId)
    {
        await using var ctx = await factory.CreateDbContextAsync();

        var teacher = GetTeacher(ctx, teacherId);
        
        return teacher.SelectMany(u => u.Groups)
            .Select(g => new GroupDto
            {
                Id = g.GroupID,
                Name = g.Name,
                Description = g.Description,
            }).ToList();
    }

    // Tested
    public async Task CreateGroup(Guid teacherId, ConfigGroupDto group)
    {
        await using var ctx = await factory.CreateDbContextAsync();
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
        await using var ctx = await factory.CreateDbContextAsync();
        
        var teacher = GetTeacher(ctx, teacherId);
        
        var qGroup = await teacher.SelectMany(u => u.Groups).SingleAsync(g => g.Name == group.Name);
        
        qGroup.Description = group.Description;
        qGroup.Name = group.Name;
        
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteGroup(Guid teacherId, int groupId)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        var teacher = GetTeacher(ctx, teacherId);
        var qGroup = await teacher.SelectMany(u => u.Groups)
            .Include(g => g.Users)
            .SingleAsync(g => g.GroupID == groupId);
       
        qGroup.Users.Remove(teacher.Single());
        await ctx.SaveChangesAsync();
        
        ctx.Groups.Remove(qGroup);
        await ctx.SaveChangesAsync();
    }

    public async Task AssignUserToGroup(Guid teacherId, Guid userId, int groupId)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        var user = await ctx.Users.SingleAsync(u => u.UserID == userId);
        var teacher = GetTeacher(ctx, teacherId);
        var group = await teacher.SelectMany(u => u.Groups).SingleAsync(g => g.GroupID == groupId);
        group.Users.Add(user);
        
        await ctx.SaveChangesAsync();
    }

    public async Task RemoveUserFromGroup(Guid teacherId, Guid userId, int groupId)
    {
        await using var ctx =  await factory.CreateDbContextAsync();
        
        var user = await ctx.Users.SingleAsync(u => u.UserID == userId);
        var teacher = GetTeacher(ctx, teacherId);
        var group = await teacher
            .SelectMany(u => u.Groups)
            .Include(u => u.Users)
            .SingleAsync(g => g.GroupID == groupId);

        group.Users.Remove(user);
        
        await ctx.SaveChangesAsync();
    }
}