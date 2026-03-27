using System.ComponentModel.DataAnnotations;

namespace SpotAnalysis.Services.DTOs;

public class LoginDto
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Please provide a User Name")]
    public required string UserName { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Please provide a Password")]
    public required string Password { get; set; }
}