using Microsoft.EntityFrameworkCore;
using SpotAnalysis.Data;
using SpotAnalysis.Data.Enums;
using SpotAnalysis.Data.Models.Identity;

namespace SpotAnalysis.Services.Services;

public class UserService(IDbContextFactory<AnalysisContext> factory) : IUserService
{
    public async Task<User?> ChangePassword(string userName, string newPassword)
    {
        if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(newPassword))
        {
            return null;
        }
        await using var context = await factory.CreateDbContextAsync();
        
        var user = context.Users.FirstOrDefault(u => u.UserName == userName);
        if (user == null) return null;
      
        var newHash = new PasswordProvider.Password(newPassword, user.UserID).ParamString();
        user.PasswordHash = newHash;
        
        await context.SaveChangesAsync();
        
        return user;
    }

    public async Task<User?> Login(string userName, string password)
    {
        System.Console.WriteLine("Login method called with username: " + userName);

        if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
        {
            return null;
        }

        using var context = factory.CreateDbContext();
        var user = context.Users.FirstOrDefault(u => u.UserName == userName);
        if (user == null) return null;
        
        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            return null;
        }

        var hashedPassword = new PasswordProvider.Password(password, user.UserID);
        var storedHash = PasswordProvider.Password.FromParamString(user.PasswordHash);
        if (hashedPassword.Compare(storedHash))
        {
            return user;
        }
        return null;
    }
    
    public async Task Register(string userName, string password, Guid? userId)
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