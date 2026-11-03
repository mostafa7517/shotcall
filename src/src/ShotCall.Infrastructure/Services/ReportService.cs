using Microsoft.EntityFrameworkCore;
using ShotCall.Application.DTOs;
using ShotCall.Application.Interfaces;
using ShotCall.Domain.Enums;
using ShotCall.Infrastructure.Persistence;

namespace ShotCall.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _db;

    public ReportService(AppDbContext db) => _db = db;

    public async Task<MonthlyReportDto> GetMonthlyReportAsync(int year, int month)
    {
        var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddMonths(1);

        var matchesInMonth = await _db.Matches
            .Where(m => m.MatchDate >= start && m.MatchDate < end)
            .CountAsync();

        var acceptedRequests = await _db.MatchRequests
            .Include(r => r.Match)
            .Where(r => r.Status == RequestStatus.Accepted && r.Match.MatchDate >= start && r.Match.MatchDate < end)
            .ToListAsync();

        var photographerStats = acceptedRequests
            .GroupBy(r => new { r.PhotographerId, r.Photographer.FullName })
            .Select(g => new PhotographerStatDto
            {
                PhotographerId = g.Key.PhotographerId,
                PhotographerName = g.Key.FullName,
                MatchesCovered = g.Count()
            })
            .ToList();

        // Fill in violation counts + current reliability score for each photographer that appeared
        foreach (var stat in photographerStats)
        {
            stat.Violations = await _db.Violations
                .CountAsync(v => v.PhotographerId == stat.PhotographerId &&
                                  v.CreatedAt >= start && v.CreatedAt < end);

            var user = await _db.Users.FindAsync(stat.PhotographerId);
            stat.ReliabilityScore = user?.ReliabilityScore ?? 0;
        }

        return new MonthlyReportDto
        {
            Year = year,
            Month = month,
            TotalMatches = matchesInMonth,
            TotalAcceptedRequests = acceptedRequests.Count,
            PhotographerStats = photographerStats.OrderByDescending(p => p.MatchesCovered).ToList()
        };
    }
}