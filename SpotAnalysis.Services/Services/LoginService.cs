
namespace SpotAnalysis.Services.Services;

using System.Security.Cryptography.X509Certificates;
using SpotAnalysis.Data;
using SpotAnalysis.Data.Models.Identity;

public class LoginService : ILoginService 
{
    private readonly AnalysisContext _context;
    private readonly string Salt = "S0m3R@nd0mS@lt!Because_I-Don't_Want_You_To_Crack_My_Passwords_and_i-Dont-wanna-add.a_GUID_as_salt";

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

        var hashedPassword = new ArgonProvider.ArgonOutput(password, Salt);
        var storedHash = ArgonProvider.ArgonOutput.FromParamString(user.PasswordHash);
        if (hashedPassword.Compare(storedHash))
        {
            return user;
        }
        return null;
    }
}