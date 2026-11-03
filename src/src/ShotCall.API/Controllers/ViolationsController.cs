using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShotCall.Application.DTOs;
using ShotCall.Application.Interfaces;

namespace ShotCall.API.Controllers;

[ApiController]
[Route("api/violations")]
[Authorize(Roles = "Admin")]
public class ViolationsController : ApiControllerBase
{
    private readonly IViolationService _violationService;

    public ViolationsController(IViolationService violationService) => _violationService = violationService;

    [HttpGet]
    public async Task<ActionResult<List<ViolationDto>>> GetForPhotographer([FromQuery] Guid photographerId)
    {
        var items = await _violationService.GetForPhotographerAsync(photographerId);
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<ViolationDto>> Create([FromBody] CreateViolationDto request)
    {
        try
        {
            var created = await _violationService.CreateAsync(request);
            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}