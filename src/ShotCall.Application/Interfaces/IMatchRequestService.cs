using ShotCall.Application.DTOs;

namespace ShotCall.Application.Interfaces;

public interface IMatchRequestService
{
    Task<List<MatchRequestDto>> GetAllAsync(MatchRequestFilter filter);
    Task<MatchRequestDto> CreateAsync(CreateMatchRequestDto request, Guid photographerId);
    Task<MatchRequestDto?> WithdrawAsync(Guid requestId, Guid photographerId);
    Task<MatchRequestDto?> AcceptAsync(Guid requestId);
    Task<MatchRequestDto?> RejectAsync(Guid requestId);

    // Admin appoints people manually; they have to confirm
    Task<List<AssignCandidateDto>> GetCandidatesAsync(Guid matchId);
    Task<List<MatchRequestDto>> InviteAsync(Guid matchId, List<Guid> userIds);
    Task<MatchRequestDto?> RevokeInvitationAsync(Guid requestId);

    // The invited person answers
    Task<MatchRequestDto?> ConfirmInvitationAsync(Guid requestId, Guid userId);
    Task<MatchRequestDto?> DeclineInvitationAsync(Guid requestId, Guid userId, string? reason);
}