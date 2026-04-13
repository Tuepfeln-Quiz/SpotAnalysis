namespace SpotAnalysis.Services.DTOs;

public class TeacherAdminDto
{
    public required Guid Id { get; init; }
    public required string UserName { get; init; }
    public required List<string> Roles { get; init; } = [];
}