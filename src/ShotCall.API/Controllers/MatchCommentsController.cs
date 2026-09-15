using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShotCall.Application.DTOs;
using ShotCall.Application.Interfaces;

namespace ShotCall.API.Controllers;

[ApiController]
[Route("api/matches/{matchId:guid}/comments")]
[Authorize]
public class MatchCommentsController : ApiControllerBase
{
    private readonly IMatchCommentService _commentService;

    public MatchCommentsController(IMatchCommentService commentService) => _commentService = commentService;

    [HttpGet]
    public async Task<ActionResult<List<MatchCommentDto>>> GetByMatch(Guid matchId)
    {
        var comments = await _commentService.GetByMatchAsync(matchId);
        return Ok(comments);
    }

    [HttpPost]
    public async Task<ActionResult<MatchCommentDto>> Create(Guid matchId, [FromBody] CreateMatchCommentDto request)
    {
        try
        {
            var created = await _commentService.CreateAsync(matchId, CurrentUserId, request.Message);
            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}