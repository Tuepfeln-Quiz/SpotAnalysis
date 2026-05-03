namespace SpotAnalysis.Services.DTOs;

public class ResetPasswordDto
{
    public required string UserName { get; set; }
    public required string NewPassword { get; set; }
    public required string OldPassword { get; set; }
}