using SpotAnalysis.Data.Enums;
using SpotAnalysis.Services.DTOs;

namespace SpotAnalysis.Services.Services;

public interface IAdminService
{
    Task AddRoleToUser(Guid userId, Role role);
    Task RemoveRoleFromUser(Guid userId, Role role);
    Task DeleteUser(Guid userId);
    Task<List<UserDto>> GetUsersByRole(Role role);
}