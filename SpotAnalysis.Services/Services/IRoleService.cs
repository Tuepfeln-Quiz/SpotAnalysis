namespace SpotAnalysis.Services.Services;

public interface IRoleService
{
    public void AddUserToRole(int userId, int roleId);
    public void RemoveUserFromRole(int userId, int roleId);
}