using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShotCall.Application.DTOs;
using ShotCall.Application.Interfaces;

namespace ShotCall.API.Controllers;

[ApiController]
[Route("api/matches/{matchId:guid}/photos")]
[Authorize]
public class MatchPhotosController : ApiControllerBase
{
    private readonly IMatchPhotoService _photoService;

    public MatchPhotosController(IMatchPhotoService photoService) => _photoService = photoService;

    [HttpGet]
    public async Task<ActionResult<List<MatchPhotoDto>>> GetByMatch(Guid matchId)
    {
        var photos = await _photoService.GetByMatchAsync(matchId);
        return Ok(photos);
    }

    [HttpPost]
    [Authorize(Roles = "Photographer")]
    public async Task<ActionResult<MatchPhotoDto>> Create(Guid matchId, [FromBody] CreateMatchPhotoDto request)
    {
        try
        {
            var created = await _photoService.CreateAsync(matchId, CurrentUserId, request.FileUrl);
            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}