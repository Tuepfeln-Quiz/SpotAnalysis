using System.Security.Authentication;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpotAnalysis.Data;
using SpotAnalysis.Data.Enums;
using SpotAnalysis.Data.Models.Identity;

namespace SpotAnalysis.Services.Services;

public class UserService(ILogger<UserService> logger, IDbContextFactory<AnalysisContext> factory) : IUserService
{
    private static readonly Regex PasswordRegex = new(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$",
        RegexOptions.Compiled);

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

    public async Task<User> UpdateProfile(Guid userId, string userName, string? newPassword = null)
    {
        if (string.IsNullOrWhiteSpace(userName) || userName.Trim().Length > 128)
            throw new ArgumentException("InvalidUserName");

        if (!string.IsNullOrWhiteSpace(newPassword) && !PasswordRegex.IsMatch(newPassword))
            throw new ArgumentException("WeakPassword");

        await using var context = await factory.CreateDbContextAsync();
        var normalizedUserName = userName.Trim();

        var user = await context.Users.SingleOrDefaultAsync(u => u.UserID == userId);
        if (user is null)
            throw new InvalidOperationException("UserNotFound");

        var exists = await context.Users.AnyAsync(u => u.UserID != userId && u.UserName.ToLower() == normalizedUserName.ToLower());
        if (exists)
            throw new InvalidOperationException("UserNameTaken");

        user.UserName = normalizedUserName;

        if (!string.IsNullOrWhiteSpace(newPassword))
            user.PasswordHash = new PasswordProvider.Password(newPassword, user.UserID).ParamString();

        await context.SaveChangesAsync();
        return user;
    }

    public async Task<User> Login(string userName, string password)
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
    
    public async Task Register(string userName, string password, string? email, Guid? userId)
    {
        if (string.IsNullOrWhiteSpace(userName) || userName.Length > 128)
        {
            throw new ArgumentException("InvalidUserName");
        }

        if (string.IsNullOrWhiteSpace(password) || !PasswordRegex.IsMatch(password))
        {
            throw new ArgumentException("WeakPassword");
        }

        var newGuid = userId ?? Guid.NewGuid();

        var passwordString = new PasswordProvider.Password(password, newGuid).ParamString();
        
        await using var context = await factory.CreateDbContextAsync();

        var normalizedUserName = userName.Trim();
        var exists = await context.Users.AnyAsync(u => u.UserName.ToLower() == normalizedUserName.ToLower());
        if (exists)
        {
            throw new InvalidOperationException("UserNameTaken");
        }

        var newUser = new User
        {
            UserName = normalizedUserName,
            PasswordHash = passwordString,
            UserID = newGuid
        };
        newUser.Roles.Add(Role.Student);
        context.Users.Add(newUser);
        
        await context.SaveChangesAsync();
    }
}