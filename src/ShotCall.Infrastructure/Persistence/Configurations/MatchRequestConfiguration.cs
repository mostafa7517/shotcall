using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShotCall.Domain.Entities;

namespace ShotCall.Infrastructure.Persistence.Configurations;

public class MatchRequestConfiguration : IEntityTypeConfiguration<MatchRequest>
{
    public void Configure(EntityTypeBuilder<MatchRequest> builder)
    {
        builder.HasKey(r => r.Id);

        // A photographer can only have one active request per match
        builder.HasIndex(r => new { r.MatchId, r.PhotographerId }).IsUnique();

        builder.HasOne(r => r.CancellationRequest)
            .WithOne(c => c.MatchRequest)
            .HasForeignKey<CancellationRequest>(c => c.MatchRequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}