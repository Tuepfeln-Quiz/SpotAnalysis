using SpotAnalysis.Data.Enums;
using SpotAnalysis.Services.DTOs;

namespace SpotAnalysis.Services.Services;

public interface IAdminService
{
    Task<ConfigUserDto> GetUser(Guid userId);
    Task AddRoleToUser(Guid userId, Role role);
    Task RemoveRoleFromUser(Guid userId, Role role);
    Task DeleteUser(Guid userId);
    Task<List<UserDto>> GetUsersByRole(Role role);
}
