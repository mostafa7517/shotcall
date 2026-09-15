using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShotCall.Application.DTOs;
using ShotCall.Application.Interfaces;

namespace ShotCall.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = "Admin")]
public class ReportsController : ApiControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService) => _reportService = reportService;

    // GET /api/reports/monthly?year=2026&month=9
    [HttpGet("monthly")]
    public async Task<ActionResult<MonthlyReportDto>> GetMonthly([FromQuery] int year, [FromQuery] int month)
    {
        if (month is < 1 or > 12)
            return BadRequest(new { error = "Month must be between 1 and 12." });

        var report = await _reportService.GetMonthlyReportAsync(year, month);
        return Ok(report);
    }
}