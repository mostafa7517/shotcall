using ShotCall.Domain.Enums;

namespace ShotCall.Domain.Entities;

public class MatchSlot
{
    public Guid Id { get; set; }

    public Guid MatchId { get; set; }
    public Match Match { get; set; } = null!;

    public StaffType StaffType { get; set; }
    public int Needed { get; set; }
}