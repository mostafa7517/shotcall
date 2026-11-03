using ShotCall.Application.DTOs;

namespace ShotCall.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> GoogleLoginAsync(string googleIdToken);
    Task<AuthResponseDto?> RefreshAsync(string refreshToken);
}