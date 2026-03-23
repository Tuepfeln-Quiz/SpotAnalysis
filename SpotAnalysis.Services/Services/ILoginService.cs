using Models.Identity;

namespace SpotAnalysis.Services;

public interface ILoginService
{
    public void Login(string userName, string password);
    public void ChangePassword(string userName, string oldPassword, string newPassword);
}