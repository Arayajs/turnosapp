using FluentAssertions;
using Moq;
using TurnOS.Application.DTOs.Auth;
using TurnOS.Application.Interfaces;
using TurnOS.Application.Services;
using TurnOS.Domain.Entities;
using TurnOS.Domain.Enums;
using TurnOS.Domain.Interfaces;

namespace TurnOS.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IJwtService> _jwtMock;
    private readonly Mock<IPasswordHasher> _hasherMock;
    private readonly Mock<IEmailService> _emailMock;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _jwtMock = new Mock<IJwtService>();
        _hasherMock = new Mock<IPasswordHasher>();
        _emailMock = new Mock<IEmailService>();

        _sut = new AuthService(
            _userRepoMock.Object,
            _jwtMock.Object,
            _hasherMock.Object,
            _emailMock.Object);
    }

    [Fact]
    public async Task Register_WhenEmailIsNew_ShouldCreateUserAndReturnToken()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FullName = "Juan Pérez",
            Email = "juan@test.com",
            Password = "pass123",
            Role = UserRole.Client
        };

        _userRepoMock.Setup(r => r.ExistsAsync(request.Email)).ReturnsAsync(false);
        _hasherMock.Setup(h => h.Hash(request.Password)).Returns("hashed_pass");
        _jwtMock.Setup(j => j.GenerateRefreshToken()).Returns("refresh_token");
        _jwtMock.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("jwt_token");

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("jwt_token");
        result.RefreshToken.Should().Be("refresh_token");
        result.User.Email.Should().Be("juan@test.com");
        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task Register_WhenEmailAlreadyExists_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.ExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

        var request = new RegisterRequest { Email = "existing@test.com", Password = "pass" };

        // Act
        var act = () => _sut.RegisterAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*registrado*");
    }

    [Fact]
    public async Task Login_WhenCredentialsAreValid_ShouldReturnAuthResponse()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@test.com",
            FullName = "Test User",
            PasswordHash = "hashed",
            Role = UserRole.Client
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync("user@test.com")).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify("pass123", "hashed")).Returns(true);
        _jwtMock.Setup(j => j.GenerateToken(user)).Returns("jwt_token");
        _jwtMock.Setup(j => j.GenerateRefreshToken()).Returns("refresh_token");

        // Act
        var result = await _sut.LoginAsync(new LoginRequest
        {
            Email = "user@test.com",
            Password = "pass123"
        });

        // Assert
        result.Token.Should().Be("jwt_token");
        result.User.Id.Should().Be(user.Id);
        _userRepoMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task Login_WhenUserNotFound_ShouldThrowUnauthorizedException()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        // Act
        var act = () => _sut.LoginAsync(new LoginRequest
        {
            Email = "nobody@test.com",
            Password = "pass"
        });

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Login_WhenPasswordIsWrong_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var user = new User { Email = "user@test.com", PasswordHash = "hashed" };
        _userRepoMock.Setup(r => r.GetByEmailAsync("user@test.com")).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify("wrong_pass", "hashed")).Returns(false);

        // Act
        var act = () => _sut.LoginAsync(new LoginRequest
        {
            Email = "user@test.com",
            Password = "wrong_pass"
        });

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        _userRepoMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RefreshToken_WhenTokenExpired_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var expiredUser = new User
        {
            RefreshToken = "expired_token",
            RefreshTokenExpiry = DateTime.UtcNow.AddDays(-1)
        };

        _userRepoMock.Setup(r => r.GetByRefreshTokenAsync("expired_token"))
            .ReturnsAsync(expiredUser);

        // Act
        var act = () => _sut.RefreshTokenAsync("expired_token");

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*expirado*");
    }
}
