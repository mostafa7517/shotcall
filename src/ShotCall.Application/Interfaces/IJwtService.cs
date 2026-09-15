using ShotCall.Domain.Entities;

namespace ShotCall.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(ApplicationUser user);
    string GenerateRefreshToken();
}