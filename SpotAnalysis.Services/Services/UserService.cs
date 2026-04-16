using System.Security.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpotAnalysis.Data;
using SpotAnalysis.Data.Enums;
using SpotAnalysis.Data.Models.Identity;

namespace SpotAnalysis.Services.Services;

public class UserService(ILogger<UserService> logger, IDbContextFactory<AnalysisContext> factory) : IUserService
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
        if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
        {
            logger.LogWarning("Username or password was null or empty, Username: {userName}, Password: {password}", userName, password);
            throw new ArgumentException("Username or password was null or empty");
        }

        await using var context = await factory.CreateDbContextAsync();
        var user = await context.Users.SingleOrDefaultAsync(u => u.UserName.ToLower() == userName.ToLower());
        
        if (user == null)
        {
            logger.LogWarning("No user was found with user name: {userName}", userName);
            throw new ArgumentException("No user was found with given user name");
        }
        
        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            logger.LogWarning("The password was null or empty");
            throw new ArgumentException("The given password was empty");
        }

        var hashedPassword = new PasswordProvider.Password(password, user.UserID);
        var storedHash = PasswordProvider.Password.FromParamString(user.PasswordHash);

        var isPasswordCorrect = hashedPassword.Compare(storedHash);

        if (!isPasswordCorrect)
        {
            logger.LogError("The given password was wrong");
            throw new AuthenticationException("The given password was wrong");
        }

        return user;
    }
    
    public async Task Register(string userName, string password, string? email = null, Guid? userId = null)
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