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

    public async Task<User> ChangePassword(Guid userId, string oldPassword, string newPassword)
    {
        ArgumentNullException.ThrowIfNull(oldPassword);
        ArgumentNullException.ThrowIfNull(newPassword);

        await using var context = await factory.CreateDbContextAsync();

        var user = await context.Users.SingleAsync(u => u.UserID == userId);

        var correctOldPassword = PasswordProvider.Password.FromParamString(user.PasswordHash)
            .Compare(new PasswordProvider.Password(oldPassword, user.UserID));

        if (!correctOldPassword)
            throw new AuthenticationException("WrongOldPassword");

        var newHash = new PasswordProvider.Password(newPassword, user.UserID).ParamString();
        user.PasswordHash = newHash;

        await context.SaveChangesAsync();

        return user;
    }

    public async Task<User> UpdateUsername(Guid userId, string newUsername)
    {
        ArgumentNullException.ThrowIfNull(newUsername);
        await using var context = await factory.CreateDbContextAsync();
        var user = await context.Users.SingleAsync(u => u.UserID == userId);

        var isUsernameTaken = await context.Users.AnyAsync(u =>
            u.UserName.Equals(newUsername, StringComparison.CurrentCultureIgnoreCase));

        if (isUsernameTaken)
        {
            throw new AuthenticationException("Username taken");
        }

        user.UserName = newUsername;
        await context.SaveChangesAsync();
        return user;
    }

    public async Task<User> Login(string userName, string password)
    {
        if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
        {
            throw new ArgumentException("Username or password was null or empty");
        }

        await using var context = await factory.CreateDbContextAsync();
        var user = await context.Users.SingleOrDefaultAsync(u => u.UserName.ToLower() == userName.ToLower());

        if (user == null)
        {
            throw new ArgumentException("No user was found with given user name");
        }

        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            throw new ArgumentException("The given password was empty");
        }

        var hashedPassword = new PasswordProvider.Password(password, user.UserID);
        var storedHash = PasswordProvider.Password.FromParamString(user.PasswordHash);

        var isPasswordCorrect = hashedPassword.Compare(storedHash);

        if (!isPasswordCorrect)
        {
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