using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShotCall.Domain.Entities;

namespace ShotCall.Infrastructure.Persistence.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.FullName).IsRequired().HasMaxLength(150);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(200);
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.PhoneNumber).HasMaxLength(30);
        builder.Property(u => u.Bio).HasMaxLength(1000);

        builder.HasMany(u => u.Availabilities)
            .WithOne(a => a.Photographer)
            .HasForeignKey(a => a.PhotographerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.MatchRequests)
            .WithOne(r => r.Photographer)
            .HasForeignKey(r => r.PhotographerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.Violations)
            .WithOne(v => v.Photographer)
            .HasForeignKey(v => v.PhotographerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}