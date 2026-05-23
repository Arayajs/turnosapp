using System.ComponentModel.DataAnnotations;
using TurnOS.Domain.Enums;

namespace TurnOS.Application.DTOs.Auth;

public class RegisterRequest
{
    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Client;
}
