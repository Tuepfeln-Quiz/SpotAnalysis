using SpotAnalysis.Data.Enums;

namespace SpotAnalysis.Services.DTOs;

public class ConfigUserDto
{
    public required string UserName { get; set; }
    public required List<Role> Roles { get; set; }
}