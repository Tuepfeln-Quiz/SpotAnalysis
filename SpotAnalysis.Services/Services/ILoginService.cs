namespace SpotAnalysis_Api.Services;

public interface ILoginService
{
    public void Login(string userName, string password);
    public void ChangePassword(string userName, string oldPassword, string newPassword);
}