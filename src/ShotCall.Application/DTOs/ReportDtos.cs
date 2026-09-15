namespace ShotCall.Application.DTOs;

public class MonthlyReportDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int TotalMatches { get; set; }
    public int TotalAcceptedRequests { get; set; }
    public List<PhotographerStatDto> PhotographerStats { get; set; } = new();
}

public class PhotographerStatDto
{
    public Guid PhotographerId { get; set; }
    public string PhotographerName { get; set; } = string.Empty;
    public int MatchesCovered { get; set; }
    public int Violations { get; set; }
    public int ReliabilityScore { get; set; }
}