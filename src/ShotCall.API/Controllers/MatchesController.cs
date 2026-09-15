using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShotCall.Application.DTOs;
using ShotCall.Application.Interfaces;

namespace ShotCall.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MatchesController : ApiControllerBase
{
    private readonly IMatchService _matchService;

    public MatchesController(IMatchService matchService)
    {
        _matchService = matchService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<MatchDto>>> GetAll([FromQuery] MatchFilter filter)
    {
        var matches = await _matchService.GetAllAsync(filter);
        return Ok(matches);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<MatchDto>> GetById(Guid id)
    {
        var match = await _matchService.GetByIdAsync(id);
        return match is null ? NotFound() : Ok(match);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<MatchDto>> Create([FromBody] CreateMatchRequest request)
    {
        var created = await _matchService.CreateAsync(request, CurrentUserId);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<MatchDto>> Update(Guid id, [FromBody] UpdateMatchRequest request)
    {
        var updated = await _matchService.UpdateAsync(id, request);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _matchService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}