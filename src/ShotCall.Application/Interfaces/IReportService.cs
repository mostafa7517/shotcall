using ShotCall.Application.DTOs;

namespace ShotCall.Application.Interfaces;

public interface IReportService
{
    Task<MonthlyReportDto> GetMonthlyReportAsync(int year, int month);
}