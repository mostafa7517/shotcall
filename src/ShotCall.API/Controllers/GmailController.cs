using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShotCall.Application.Interfaces;

namespace ShotCall.API.Controllers;

[ApiController]
[Route("api/gmail")]
public class GmailController : ApiControllerBase
{
    private readonly IGmailAuthService _gmailAuthService;

    public GmailController(IGmailAuthService gmailAuthService) => _gmailAuthService = gmailAuthService;

    // GET /api/gmail/connect - Admin opens this URL in a browser to start the flow
    [HttpGet("connect")]
    [AllowAnonymous]
    public IActionResult Connect()
    {
        var url = _gmailAuthService.GetAuthorizationUrl();
        return Redirect(url);
    }
    // GET /api/gmail/callback - Google redirects here after consent
    // NOTE: this endpoint can't easily use [Authorize] because the browser redirect from Google
    // doesn't carry our JWT. For now it trusts a hardcoded admin lookup - see note below.
    [HttpGet("callback")]
    [AllowAnonymous]
    public async Task<IActionResult> Callback([FromQuery] string code, [FromServices] Infrastructure.Persistence.AppDbContext db)
    {
        // Simplification: attach the token to the first Admin user found.
        // Fine for a single-admin setup; extend later if multiple admins need their own Gmail connection.
        var admin = db.Users.FirstOrDefault(u => u.Role == Domain.Enums.UserRole.Admin);
        if (admin is null) return BadRequest("No admin account found.");

        var result = await _gmailAuthService.HandleCallbackAsync(code, admin.Id);
        return Content(result);
    }
}