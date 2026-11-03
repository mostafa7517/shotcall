using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShotCall.Domain.Entities;

namespace ShotCall.Infrastructure.Persistence.Configurations;

public class WaitingListEntryConfiguration : IEntityTypeConfiguration<WaitingListEntry>
{
    public void Configure(EntityTypeBuilder<WaitingListEntry> builder)
    {
        builder.HasKey(w => w.Id);
        builder.HasIndex(w => new { w.MatchId, w.PhotographerId }).IsUnique();

        builder.HasOne(w => w.Photographer)
            .WithMany()
            .HasForeignKey(w => w.PhotographerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}