namespace ShotCall.Application.DTOs;

public class AvailabilityDto
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly FromTime { get; set; }
    public TimeOnly ToTime { get; set; }
}

public class CreateAvailabilityDto
{
    public DateOnly Date { get; set; }
    public TimeOnly FromTime { get; set; }
    public TimeOnly ToTime { get; set; }
}