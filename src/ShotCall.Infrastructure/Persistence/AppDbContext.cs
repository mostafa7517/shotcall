using Microsoft.EntityFrameworkCore;
using ShotCall.Domain.Entities;

namespace ShotCall.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<Availability> Availabilities => Set<Availability>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<MatchRequest> MatchRequests => Set<MatchRequest>();
    public DbSet<CancellationRequest> CancellationRequests => Set<CancellationRequest>();
    public DbSet<WaitingListEntry> WaitingListEntries => Set<WaitingListEntry>();
    public DbSet<Violation> Violations => Set<Violation>();
    public DbSet<MatchComment> MatchComments => Set<MatchComment>();
    public DbSet<MatchPhoto> MatchPhotos => Set<MatchPhoto>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<GmailAuthorization> GmailAuthorizations => Set<GmailAuthorization>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}