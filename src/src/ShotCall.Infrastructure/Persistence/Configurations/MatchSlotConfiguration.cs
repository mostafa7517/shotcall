using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShotCall.Domain.Entities;

namespace ShotCall.Infrastructure.Persistence.Configurations;

public class MatchSlotConfiguration : IEntityTypeConfiguration<MatchSlot>
{
    public void Configure(EntityTypeBuilder<MatchSlot> builder)
    {
        builder.HasKey(s => s.Id);

        // One slot per role type per match
        builder.HasIndex(s => new { s.MatchId, s.StaffType }).IsUnique();
    }
}