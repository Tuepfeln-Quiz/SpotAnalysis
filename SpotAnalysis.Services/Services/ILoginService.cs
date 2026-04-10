using SpotAnalysis.Data.Models.Identity;

namespace SpotAnalysis.Services.Services;

public interface ILoginService
{
    public User? Login(string userName, string password);
    public User ChangePassword(string userName, string oldPassword, string newPassword);
}