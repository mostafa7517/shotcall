using Microsoft.EntityFrameworkCore;
using ShotCall.Application.DTOs;
using ShotCall.Application.Interfaces;
using ShotCall.Domain.Entities;
using ShotCall.Domain.Enums;
using ShotCall.Infrastructure.Persistence;

namespace ShotCall.Infrastructure.Services;

public class AccountService : IAccountService
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notificationService;

    public AccountService(AppDbContext db, INotificationService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    public async Task<List<AccountDto>> GetAllAsync(AccountFilter filter)
    {
        var query = _db.Users.Where(u => u.Role == UserRole.Photographer).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Status) &&
            Enum.TryParse<AccountStatus>(filter.Status, true, out var status))
            query = query.Where(u => u.AccountStatus == status);

        var users = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
        return users.Select(ToDto).ToList();
    }

    public async Task<AccountDto?> ApproveAsync(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return null;
        if (user.AccountStatus != AccountStatus.PendingApproval)
            throw new InvalidOperationException("Only pending accounts can be approved.");

        user.AccountStatus = AccountStatus.Active;
        await _db.SaveChangesAsync();

        await _notificationService.CreateAsync(user.Id, NotificationType.AccountApproved,
            "Your account has been approved. You can now browse and request matches.");

        return ToDto(user);
    }

    public async Task<AccountDto?> RejectAsync(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return null;
        if (user.AccountStatus != AccountStatus.PendingApproval)
            throw new InvalidOperationException("Only pending accounts can be rejected.");

        user.AccountStatus = AccountStatus.Rejected;
        await _db.SaveChangesAsync();

        await _notificationService.CreateAsync(user.Id, NotificationType.AccountRejected,
            "Your account registration was not approved.");

        return ToDto(user);
    }

    public async Task<AccountDto?> DisableAsync(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return null;
        if (user.AccountStatus != AccountStatus.Active)
            throw new InvalidOperationException("Only active accounts can be disabled.");

        user.AccountStatus = AccountStatus.Disabled;
        await _db.SaveChangesAsync();

        return ToDto(user);
    }

    public async Task<AccountDto?> EnableAsync(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return null;
        if (user.AccountStatus != AccountStatus.Disabled)
            throw new InvalidOperationException("Only disabled accounts can be re-enabled.");

        user.AccountStatus = AccountStatus.Active;
        await _db.SaveChangesAsync();

        return ToDto(user);
    }

    private static AccountDto ToDto(ApplicationUser u) => new()
    {
        Id = u.Id,
        FullName = u.FullName,
        Email = u.Email,
        ProfilePictureUrl = u.ProfilePictureUrl,
        PhoneNumber = u.PhoneNumber,
        Bio = u.Bio,
        Role = u.Role.ToString(),
        AccountStatus = u.AccountStatus.ToString(),
        ReliabilityScore = u.ReliabilityScore,
        CreatedAt = u.CreatedAt
    };
}