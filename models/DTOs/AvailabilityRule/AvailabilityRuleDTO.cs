namespace Reservae.Models.DTOs;

public class AvailabilityRuleDto
{
    public int Id { get; set; }
    public required string SpaceName { get; set; }
    public DayOfTheWeekEnum DayOfTheWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidUntil { get; set; }
    public decimal? CustomPricePerSpot { get; set; }
    public bool IsActive { get; set; }
    public int Capacity { get; set; }
}
