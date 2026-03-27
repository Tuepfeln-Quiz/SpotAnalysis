namespace SpotAnalysis.Services.Services;

public interface IRoleService
{
    public void AddRoleToUser(int userId, int roleId);
    public void RemoveRoleFromUser(int userId, int roleId);
}