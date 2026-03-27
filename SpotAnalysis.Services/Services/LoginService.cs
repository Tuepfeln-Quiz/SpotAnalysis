
namespace SpotAnalysis.Services.Services;

using System.Security.Cryptography.X509Certificates;
using SpotAnalysis.Data;
using SpotAnalysis.Data.Models.Identity;

public class LoginService : ILoginService 
{
    private readonly AnalysisContext _context;

    public LoginService(AnalysisContext context)
    {
        _context = context;
    }

    public User ChangePassword(string userName, string oldPassword, string newPassword)
    {
        System.Console.WriteLine("ChangePassword method called with username: " + userName + ", old password: " + oldPassword + " and new password: " + newPassword);
        return null;
    }

    public User? Login(string userName, string password)
    {
        System.Console.WriteLine("Login method called with username: " + userName + " and password: " + password);

        var user = _context.Users.FirstOrDefault(u => u.UserName == userName);
        if (user == null) return null;

        var hashedPassword = new PasswordProvider.Password(password, user.UserID);
        var storedHash = PasswordProvider.Password.FromParamString(user.PasswordHash);
        if (hashedPassword.Compare(storedHash))
        {
            return user;
        }
        return null;
    }
}