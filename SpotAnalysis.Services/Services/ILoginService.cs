using SpotAnalysis.Data.Models.Identity;

namespace SpotAnalysis.Services.Services;

public interface ILoginService
{
    public Task<User?> Login(string userName, string password);
    public Task<User?> ChangePassword(string userName, string oldPassword, string newPassword);
}