using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        return Ok(new ProfileDto
        {
            FullName = user.FullName,
            Email = user.Email,
            ProfilePictureUrl = user.ProfilePictureUrl,
            PhoneNumber = user.PhoneNumber,
            Bio = user.Bio,
            ReliabilityScore = user.ReliabilityScore
        });
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

        return Ok(new ProfileDto
        {
            FullName = user.FullName,
            Email = user.Email,
            ProfilePictureUrl = user.ProfilePictureUrl,
            PhoneNumber = user.PhoneNumber,
            Bio = user.Bio,
            ReliabilityScore = user.ReliabilityScore
        });
    }
}