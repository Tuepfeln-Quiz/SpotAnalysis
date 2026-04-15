using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpotAnalysis.Data;
using SpotAnalysis.Data.Enums;
using SpotAnalysis.Services.DTOs;

namespace SpotAnalysis.Services.Services;

public class AdminService(IDbContextFactory<AnalysisContext> contextFactory, ILogger<AdminService> logger) : IAdminService
{
    public async Task AddRoleToUser(Guid userId, Role role)
    {
        try
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();

            var user = await dbContext.Users.SingleAsync(x => x.UserID == userId);

            if (!user.Roles.Contains(role))
            {
                user.Roles.Add(role);

                await dbContext.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while adding {role} to user with id {guid}.", role, userId);
            throw;
        }
    }

    public async Task RemoveRoleFromUser(Guid userId, Role role)
    {
        try
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();

            var user = await dbContext.Users.SingleAsync(x => x.UserID == userId);

            if(user.Roles.Count == 1 && user.Roles.Contains(role))
            {
                throw new InvalidOperationException($"Cannot remove the only role from user with id {userId}.");
            }

            user.Roles.Remove(role);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while removing {role} from user with id {guid}.", role, userId);
            throw;
        }
    }

    /// <summary>
    /// This will delete the user and all quiz attempts of the user as well as all groups the user is referenced in. 
    /// It will not delete any quizzes created by the user, but it will set the creator of those quizzes to null. 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="role"></param>
    /// <returns></returns>
    public async Task DeleteUser(Guid userId)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync();

        await dbContext.QuizAttempts.Where(x => x.UserID == userId).ExecuteDeleteAsync();
        await dbContext.Users.Where(x => x.UserID == userId).ExecuteDeleteAsync();
    }

    public async Task<List<UserDto>> GetUsersByRole(Role role)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync();

        return await dbContext.Users
            .Where(u => u.Roles.Contains(role))
            .Select(u => new UserDto
            {
                Id = u.UserID,
                UserName = u.UserName,
                Roles = u.Roles.Select(r => r.ToString()).ToList(),
                AssignedGroups = u.Groups.Select(g => new GroupDto
                {
                    Id = g.GroupID,
                    Name = g.Name
                }).ToList()
            })
            .ToListAsync();
    }

    public async Task<List<UserDto>> GetUsersWithoutRole()
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync();

        return await dbContext.Users
            .Where(u => u.Roles.Count == 0)
            .Select(u => new UserDto
            {
                Id = u.UserID,
                UserName = u.UserName,
                AssignedGroups = u.Groups.Select(g => new GroupDto
                {
                    Id = g.GroupID,
                    Name = g.Name
                }).ToList()
            })
            .ToListAsync();
    }

    public async Task AddUserToGroup(Guid userId, int groupId)
    {
        try
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();

            var group = await dbContext.Groups.SingleAsync(x => x.GroupID == groupId);
            var user = await dbContext.Users.SingleAsync(x => x.UserID == userId);

            if (!user.Groups.Contains(group))
            {
                user.Groups.Add(group);
                await dbContext.SaveChangesAsync();
            }
        }
        catch (Exception)
        {
            logger.LogError("An error occurred while adding user with id {guid} to group with id {groupId}.", userId, groupId);
            throw;
        }
    }

    public async Task RemoveUserFromGroup(Guid userId, int groupId)
    {
        try
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();

            var group = await dbContext.Groups.SingleAsync(x => x.GroupID == groupId);
            var user = await dbContext.Users.SingleAsync(x => x.UserID == userId);

            user.Groups.Remove(group);

            await dbContext.SaveChangesAsync();
        }
        catch (Exception)
        {
            logger.LogError("An error occurred while removing user with id {guid} from group with id {groupId}.", userId, groupId);
            throw;
        }
    }
}
