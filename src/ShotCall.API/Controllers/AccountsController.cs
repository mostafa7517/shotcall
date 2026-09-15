using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShotCall.Application.DTOs;
using ShotCall.Application.Interfaces;

namespace ShotCall.API.Controllers;

[ApiController]
[Route("api/accounts")]
[Authorize(Roles = "Admin")]
public class AccountsController : ApiControllerBase
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AccountDto>>> GetAll([FromQuery] AccountFilter filter)
    {
        var accounts = await _accountService.GetAllAsync(filter);
        return Ok(accounts);
    }

    [HttpPut("{id:guid}/approve")]
    public async Task<ActionResult<AccountDto>> Approve(Guid id)
    {
        try
        {
            var result = await _accountService.ApproveAsync(id);
            return result is null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}/reject")]
    public async Task<ActionResult<AccountDto>> Reject(Guid id)
    {
        try
        {
            var result = await _accountService.RejectAsync(id);
            return result is null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}/disable")]
    public async Task<ActionResult<AccountDto>> Disable(Guid id)
    {
        try
        {
            var result = await _accountService.DisableAsync(id);
            return result is null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}/enable")]
    public async Task<ActionResult<AccountDto>> Enable(Guid id)
    {
        try
        {
            var result = await _accountService.EnableAsync(id);
            return result is null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}