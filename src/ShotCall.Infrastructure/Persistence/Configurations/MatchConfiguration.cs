using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShotCall.Domain.Entities;

namespace ShotCall.Infrastructure.Persistence.Configurations;

public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Category).IsRequired().HasMaxLength(50);
        builder.Property(m => m.Location).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Description).HasMaxLength(2000);

        builder.HasOne(m => m.CreatedByAdmin)
            .WithMany()
            .HasForeignKey(m => m.CreatedByAdminId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(m => m.Requests)
            .WithOne(r => r.Match)
            .HasForeignKey(r => r.MatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.WaitingList)
            .WithOne(w => w.Match)
            .HasForeignKey(w => w.MatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Comments)
            .WithOne(c => c.Match)
            .HasForeignKey(c => c.MatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Photos)
            .WithOne(p => p.Match)
            .HasForeignKey(p => p.MatchId)
            .OnDelete(DeleteBehavior.Cascade);

        // Computed property, not mapped to a column
        builder.Ignore(m => m.AcceptedCount);
    }
}