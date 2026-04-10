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
    public async Task CreateAdmin(ConfigUserDto user)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync();

        Role? adminRole;
        try
        {
            adminRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Title == "xxx");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving the admin role from the database.");
            throw;
        }
        
        //dbContext.Users.Add(new User
        //{
        //    Roles = ,
        //});

        throw new NotImplementedException();
    }

    public void CreateTeacher(ConfigUserDto user)
    {
        throw new NotImplementedException();
    }

    public void DeleteAdmin(int adminId)
    {
        throw new NotImplementedException();
    }

    public void DeleteTeacher(int teacherId)
    {
        throw new NotImplementedException();
    }

    public List<TeacherAdminDto> GetAdmins()
    {
        throw new NotImplementedException();
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
