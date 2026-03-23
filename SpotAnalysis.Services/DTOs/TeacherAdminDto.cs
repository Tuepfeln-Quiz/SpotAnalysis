namespace SpotAnalysis.Services.DTOs;

public class TeacherAdminDto
{
    public required int Id { get; init; }
    public required string UserName { get; init; }
    public List<RoleDto> Roles { get; init; }
}