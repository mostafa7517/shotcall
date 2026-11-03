using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShotCall.Application.DTOs;
using ShotCall.Domain.Entities;
using ShotCall.Domain.Enums;
using ShotCall.Infrastructure.Persistence;

namespace ShotCall.API.Controllers;

public class ProfileDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Bio { get; set; }
    public int ReliabilityScore { get; set; }
    public string AccountStatus { get; set; } = string.Empty;
    public string? StaffType { get; set; }
    public bool CanChangeStaffType { get; set; }
}

public class UpdateProfileDto
{
    public string? PhoneNumber { get; set; }
    public string? Bio { get; set; }
    public string? ProfilePictureUrl { get; set; }
}

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ApiControllerBase
{
    private readonly AppDbContext _db;

    public ProfileController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<ProfileDto>> Get()
    {
        var user = await _db.Users.FindAsync(CurrentUserId);
        if (user is null) return NotFound();

        return Ok(ToDto(user));
    }

    [HttpPut]
    public async Task<ActionResult<ProfileDto>> Update([FromBody] UpdateProfileDto request)
    {
        var user = await _db.Users.FindAsync(CurrentUserId);
        if (user is null) return NotFound();

        user.PhoneNumber = request.PhoneNumber;
        user.Bio = request.Bio;
        if (!string.IsNullOrWhiteSpace(request.ProfilePictureUrl))
            user.ProfilePictureUrl = request.ProfilePictureUrl;

        await _db.SaveChangesAsync();

        return Ok(ToDto(user));
    }

    // A person picks their role while waiting for approval (or if they never picked one).
    // After approval only an admin can change it.
    [HttpPut("staff-type")]
    public async Task<ActionResult<ProfileDto>> SetStaffType([FromBody] SetStaffTypeRequest request)
    {
        var user = await _db.Users.FindAsync(CurrentUserId);
        if (user is null) return NotFound();

        if (user.Role != UserRole.Staff)
            return BadRequest(new { error = "Only staff accounts have a role type." });

        if (!CanChangeStaffType(user))
            return BadRequest(new { error = "Your role was set when you were approved. Ask an admin to change it." });

        if (!Enum.TryParse<StaffType>(request.StaffType, true, out var type) || !Enum.IsDefined(type))
            return BadRequest(new { error = "Unknown role type." });

        user.StaffType = type;
        await _db.SaveChangesAsync();

        return Ok(ToDto(user));
    }

    private static bool CanChangeStaffType(ApplicationUser user) =>
        user.Role == UserRole.Staff &&
        (user.AccountStatus == AccountStatus.PendingApproval || user.StaffType is null);

    private static ProfileDto ToDto(ApplicationUser user) => new()
    {
        FullName = user.FullName,
        Email = user.Email,
        ProfilePictureUrl = user.ProfilePictureUrl,
        PhoneNumber = user.PhoneNumber,
        Bio = user.Bio,
        ReliabilityScore = user.ReliabilityScore,
        AccountStatus = user.AccountStatus.ToString(),
        StaffType = user.StaffType?.ToString(),
        CanChangeStaffType = CanChangeStaffType(user)
    };
}