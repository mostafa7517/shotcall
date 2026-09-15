using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShotCall.Application.DTOs;
using ShotCall.Application.Interfaces;

namespace ShotCall.API.Controllers;

[ApiController]
[Route("api/requests")]
[Authorize]
public class MatchRequestsController : ApiControllerBase
{
    private readonly IMatchRequestService _requestService;

    public MatchRequestsController(IMatchRequestService requestService)
    {
        _requestService = requestService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<MatchRequestDto>>> GetAll([FromQuery] MatchRequestFilter filter)
    {
        var requests = await _requestService.GetAllAsync(filter);
        return Ok(requests);
    }

    [HttpGet("my")]
    public async Task<ActionResult<List<MatchRequestDto>>> GetMine()
    {
        var requests = await _requestService.GetAllAsync(new MatchRequestFilter { PhotographerId = CurrentUserId });
        return Ok(requests);
    }

    [HttpPost]
    [Authorize(Roles = "Photographer")]
    public async Task<ActionResult<MatchRequestDto>> Create([FromBody] CreateMatchRequestDto request)
    {
        try
        {
            var created = await _requestService.CreateAsync(request, CurrentUserId);
            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}/withdraw")]
    [Authorize(Roles = "Photographer")]
    public async Task<ActionResult<MatchRequestDto>> Withdraw(Guid id)
    {
        try
        {
            var result = await _requestService.WithdrawAsync(id, CurrentUserId);
            return result is null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}/accept")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<MatchRequestDto>> Accept(Guid id)
    {
        try
        {
            var result = await _requestService.AcceptAsync(id);
            return result is null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<MatchRequestDto>> Reject(Guid id)
    {
        try
        {
            var result = await _requestService.RejectAsync(id);
            return result is null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}