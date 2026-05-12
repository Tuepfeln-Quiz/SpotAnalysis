using SpotAnalysis.Data.Models.Identity;

namespace SpotAnalysis.Services.Services;

public interface IUserService
{
    Task Register(string userName, string password, string? email = null, Guid? userId = null);
    Task<User> Login(string userName, string password);
    Task<User> ChangePassword(Guid userId, string oldPassword, string newPassword);
    Task<User> UpdateUsername(Guid userId, string newUsername);
}
