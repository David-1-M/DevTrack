
using System.ComponentModel.DataAnnotations;

namespace DevTrackAPI.DTOs;

public class RegisterUserDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(12)]
    public string Password { get; set; } = string.Empty;
}
