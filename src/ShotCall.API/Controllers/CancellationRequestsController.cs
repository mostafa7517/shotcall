using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShotCall.Application.DTOs;
using ShotCall.Application.Interfaces;

namespace ShotCall.API.Controllers;

[ApiController]
[Route("api/cancellations")]
[Authorize]
public class CancellationRequestsController : ApiControllerBase
{
    private readonly ICancellationRequestService _cancellationService;

    public CancellationRequestsController(ICancellationRequestService cancellationService)
    {
        _cancellationService = cancellationService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<CancellationRequestDto>>> GetPending()
    {
        var items = await _cancellationService.GetPendingAsync();
        return Ok(items);
    }

    [HttpPost]
    [Authorize(Roles = "Photographer")]
    public async Task<ActionResult<CancellationRequestDto>> Create([FromBody] CreateCancellationRequestDto request)
    {
        try
        {
            var created = await _cancellationService.CreateAsync(request, CurrentUserId);
            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CancellationRequestDto>> Approve(Guid id)
    {
        try
        {
            var result = await _cancellationService.ApproveAsync(id);
            return result is null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CancellationRequestDto>> Reject(Guid id)
    {
        try
        {
            var result = await _cancellationService.RejectAsync(id);
            return result is null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}