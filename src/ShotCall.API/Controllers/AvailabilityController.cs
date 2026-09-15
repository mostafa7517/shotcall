using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShotCall.Application.DTOs;
using ShotCall.Application.Interfaces;

namespace ShotCall.API.Controllers;

[ApiController]
[Route("api/availability")]
[Authorize(Roles = "Photographer")]
public class AvailabilityController : ApiControllerBase
{
    private readonly IAvailabilityService _availabilityService;

    public AvailabilityController(IAvailabilityService availabilityService) => _availabilityService = availabilityService;

    [HttpGet]
    public async Task<ActionResult<List<AvailabilityDto>>> GetMine()
    {
        var items = await _availabilityService.GetForPhotographerAsync(CurrentUserId);
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<AvailabilityDto>> Create([FromBody] CreateAvailabilityDto request)
    {
        var created = await _availabilityService.CreateAsync(CurrentUserId, request);
        return Ok(created);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _availabilityService.DeleteAsync(id, CurrentUserId);
        return success ? NoContent() : NotFound();
    }
}