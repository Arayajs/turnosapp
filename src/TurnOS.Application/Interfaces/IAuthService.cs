using TurnOS.Application.DTOs.Auth;

namespace TurnOS.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    Task ForgotPasswordAsync(string email);
    Task ResetPasswordAsync(ResetPasswordRequest request);
}
