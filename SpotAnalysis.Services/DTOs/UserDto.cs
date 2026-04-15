namespace SpotAnalysis.Services.DTOs;

public class UserDto
{
    public required Guid Id { get; init; }
    public required string UserName { get; init; }
    public List<string> Roles { get; set; } = [];
    public List<GroupDto> AssignedGroups { get; init; } = [];
}
