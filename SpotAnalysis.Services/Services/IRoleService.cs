namespace SpotAnalysis.Services.Services;

public interface IRoleService
{
    public void AddRoleToUser(Guid userId, int roleId);
    public void RemoveRoleFromUser(Guid userId, int roleId);
}