
namespace SpotAnalysis.Services.Services;

using Microsoft.EntityFrameworkCore;
using SpotAnalysis.Data;
using SpotAnalysis.Data.Models.Identity;

public class LoginService : ILoginService
{
    private readonly IDbContextFactory<AnalysisContext> _contextFactory;

    public LoginService(IDbContextFactory<AnalysisContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<User?> ChangePassword(string userName, string oldPassword, string newPassword)
    {
        System.Console.WriteLine("ChangePassword method called with username: " + userName);
        return null;
    }

    public async Task<User?> Login(string userName, string password)
    {
        System.Console.WriteLine("Login method called with username: " + userName);

        if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
        {
            return null;
        }

        using var context = _contextFactory.CreateDbContext();
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
}