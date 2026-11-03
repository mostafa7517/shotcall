namespace ShotCall.Domain.Enums;

// New values are only ever added at the END: the numbers are stored in the database.
public enum RequestStatus
{
    Pending,
    Accepted,
    Rejected,
    Withdrawn,
    Invited,   // an admin picked this person; waiting for them to confirm
    Declined   // the person said no to an invitation
}
