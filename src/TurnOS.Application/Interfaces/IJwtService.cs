using TurnOS.Domain.Entities;

namespace TurnOS.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
    string GenerateRefreshToken();
}
