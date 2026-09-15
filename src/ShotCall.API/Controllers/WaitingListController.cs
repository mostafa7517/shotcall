using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShotCall.Application.DTOs;
using ShotCall.Application.Interfaces;

namespace ShotCall.API.Controllers;

[ApiController]
[Route("api/matches/{matchId:guid}/waitinglist")]
[Authorize]
public class WaitingListController : ApiControllerBase
{
    private readonly IWaitingListService _waitingListService;

    public WaitingListController(IWaitingListService waitingListService) => _waitingListService = waitingListService;

    [HttpGet]
    public async Task<ActionResult<List<WaitingListEntryDto>>> GetForMatch(Guid matchId)
    {
        var items = await _waitingListService.GetForMatchAsync(matchId);
        return Ok(items);
    }

    [HttpPost]
    [Authorize(Roles = "Photographer")]
    public async Task<ActionResult<WaitingListEntryDto>> Join(Guid matchId)
    {
        try
        {
            var created = await _waitingListService.JoinAsync(matchId, CurrentUserId);
            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}