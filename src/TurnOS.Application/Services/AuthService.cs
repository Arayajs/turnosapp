using TurnOS.Application.DTOs.Auth;
using TurnOS.Application.Interfaces;
using TurnOS.Domain.Entities;
using TurnOS.Domain.Interfaces;

namespace TurnOS.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailService _emailService;

    public AuthService(
        IUserRepository userRepo,
        IJwtService jwtService,
        IPasswordHasher passwordHasher,
        IEmailService emailService)
    {
        _userRepo = userRepo;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _userRepo.ExistsAsync(request.Email))
            throw new InvalidOperationException("El email ya está registrado.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            Phone = request.Phone,
            Role = request.Role
        };

        var refreshToken = _jwtService.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        await _userRepo.AddAsync(user);
        return BuildAuthResponse(user, refreshToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email.ToLowerInvariant())
            ?? throw new UnauthorizedAccessException("Credenciales inválidas.");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Credenciales inválidas.");

        var refreshToken = _jwtService.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _userRepo.UpdateAsync(user);

        return BuildAuthResponse(user, refreshToken);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var user = await _userRepo.GetByRefreshTokenAsync(refreshToken)
            ?? throw new UnauthorizedAccessException("Refresh token inválido.");

        if (user.RefreshTokenExpiry < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token expirado.");

        var newRefreshToken = _jwtService.GenerateRefreshToken();
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _userRepo.UpdateAsync(user);

        return BuildAuthResponse(user, newRefreshToken);
    }

    public async Task ForgotPasswordAsync(string email)
    {
        var user = await _userRepo.GetByEmailAsync(email.ToLowerInvariant());
        if (user is null) return; // No revelar si el email existe

        user.PasswordResetToken = Guid.NewGuid().ToString("N");
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        await _userRepo.UpdateAsync(user);

        await _emailService.SendPasswordResetAsync(user.Email, user.PasswordResetToken);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email.ToLowerInvariant())
            ?? throw new InvalidOperationException("Token inválido.");

        if (user.PasswordResetToken != request.Token || user.PasswordResetTokenExpiry < DateTime.UtcNow)
            throw new InvalidOperationException("Token inválido o expirado.");

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        await _userRepo.UpdateAsync(user);
    }

    private AuthResponse BuildAuthResponse(User user, string refreshToken)
    {
        var token = _jwtService.GenerateToken(user);
        return new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            User = new UserInfo
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString()
            }
        };
    }
}
