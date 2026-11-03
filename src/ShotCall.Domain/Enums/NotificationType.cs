namespace ShotCall.Domain.Enums;

// New values are only ever added at the END: the numbers are stored in the database.
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
    MatchCancelled,
    MatchInvitation,
    InvitationConfirmed,
    InvitationDeclined,
    InvitationRevoked
}
