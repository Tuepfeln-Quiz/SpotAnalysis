using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpotAnalysis.Data;
using SpotAnalysis.Data.Models.Identity;
using SpotAnalysis.Services.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpotAnalysis.Services.Services;

public class AdminService(IDbContextFactory<AnalysisContext> contextFactory, ILogger<AdminService> logger) : IAdminService
{
    public async Task AddRoleToUser(Guid userId, string role)
    {
        try
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();

            var user = await dbContext.Users.SingleAsync(x => x.UserID == userId);

            var roleToAdd = await dbContext.Roles.SingleAsync(x => x.Title.ToLower() == role.ToLower());
            user.Roles.Add(roleToAdd);

            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while adding {role} to user with id {guid}.", role, userId);
            throw;
        }
    }

    public async Task RemoveRoleFromUser(Guid userId, string role)
    {
        try
        {
            await using var dbContext = await contextFactory.CreateDbContextAsync();

            var user = await dbContext.Users.SingleAsync(x => x.UserID == userId);

            var roleToRemove = await dbContext.Roles.SingleAsync(x => x.Title.ToLower() == role.ToLower());
            user.Roles.Remove(roleToRemove);

            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while removing {role} from user with id {guid}.", role, userId);
            throw;
        }
    }

    /// <summary>
    /// This will delete the user and all quiz attempts of the user. 
    /// It will not delete any quizzes created by the user, but it will set the creator of those quizzes to null. 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="role"></param>
    /// <returns></returns>
    public async Task DeleteUser(Guid userId, string role)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync();

        await dbContext.QuizAttempts.Where(x => x.UserID == userId).ExecuteDeleteAsync();
        await dbContext.Users.Where(x => x.UserID == userId).ExecuteDeleteAsync();
    }

    public async Task<List<TeacherAdminDto>> GetAdmins()
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync();

        return await dbContext.Users
            .Where(u => u.Roles.Any(r => r.Title.ToLower() == "admin"))
            .Select(u => new TeacherAdminDto
            {
                Id = u.UserID,
                UserName = u.UserName,
                Roles = u.Roles.Select(r => r.Title).ToList()
            })
            .ToListAsync();
    }

    public List<TeacherAdminDto> GetTeachers()
    {
        throw new NotImplementedException();
    }

    public void UpdateAdmin(ConfigUserDto user)
    {
        throw new NotImplementedException();
    }

    public void UpdateTeacher(ConfigUserDto user)
    {
        throw new NotImplementedException();
    }
}
