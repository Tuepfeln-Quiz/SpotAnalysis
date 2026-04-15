using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpotAnalysis.Data;
using SpotAnalysis.Data.Enums;
using SpotAnalysis.Data.Models.Identity;

namespace SpotAnalysis.Services.Services;

public class UserService(ILogger<UserService> logger, IDbContextFactory<AnalysisContext> factory) : IUserService
{
    public async Task<User?> ChangePassword(string userName, string oldPassword, string newPassword)
    {
        return null;
    }

    public async Task<User?> Login(string userName, string password)
    {
        if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
        {
            return null;
        }

        await using var context = await factory.CreateDbContextAsync();
        var user = context.Users.FirstOrDefault(u => u.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
        
        if (user == null) return null;
        
        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            return null;
        }

        var hashedPassword = new PasswordProvider.Password(password, user.UserID);
        var storedHash = PasswordProvider.Password.FromParamString(user.PasswordHash);
        
        return hashedPassword.Compare(storedHash) ? user : null;
    }
    
    public async Task Register(string userName, string password, string? email, Guid? userId)
    {
        var newGuid = userId ?? Guid.NewGuid();

        var passwordString = new PasswordProvider.Password(password, newGuid).ParamString();
        
        await using var context = await factory.CreateDbContextAsync();
        var newUser = new User
        {
            UserName = userName,
            PasswordHash = passwordString,
            UserID = newGuid
        };
        newUser.Roles.Add(Role.Student);
        context.Users.Add(newUser);
        
        await context.SaveChangesAsync();
    }
}