using SpotAnalysis.Data.Models.Identity;

namespace SpotAnalysis.Services.Services;

public interface IUserService
{
    public Task Register(string userName, string password, string? email = null, Guid? userId = null);
    public Task<User> Login(string userName, string password);
    public Task<User?> ChangePassword(string userName, string oldPassword, string newPassword);
}