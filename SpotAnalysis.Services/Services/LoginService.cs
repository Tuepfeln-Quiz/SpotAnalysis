
namespace SpotAnalysis.Services.Services;

public class LoginService : ILoginService , IStudentService
{
    public void ChangePassword(string userName, string oldPassword, string newPassword)
    {
        System.Console.WriteLine("ChangePassword method called with username: " + userName + ", old password: " + oldPassword + " and new password: " + newPassword);
    }

    public void Login(string userName, string password)
    {
        System.Console.WriteLine("Login method called with username: " + userName + " and password: " + password);
    }

    public void Register(string password, string? code)
    {
        System.Console.WriteLine("Register method called with password: " + password + " and code: " + code);
    }
}