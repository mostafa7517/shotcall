namespace ShotCall.Domain.Enums;

public enum NotificationType
{
    NewMatch,
    NewRequest,
    RequestAccepted,
    RequestRejected,
    AccountApproved,
    AccountRejected,
    CancellationSubmitted,
    CancellationDecision,
    NewComment,
    MatchUpdated,
    MatchCancelled
}