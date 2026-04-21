namespace SpotAnalysis.Services.DTOs;

public class ResetPasswordDto
{
    public required string UserName { get; set; }
    public required string newPassword { get; set; }
}