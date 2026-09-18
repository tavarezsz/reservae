namespace Reservae.Models.DTOs;

public class UpdateAvailabilityRuleDTO
{
    public DayOfTheWeekEnum? DayOfTheWeek { get; init; }
    public TimeOnly? StartTime { get; init; }
    public TimeOnly? EndTime { get; init; }
    public DateTime? ValidFrom { get; init; }
    public DateTime? ValidUntil { get; init; }
    public decimal? CustomPricePerSpot { get; init; }
    public bool? IsActive { get; init; }
    public int? Capacity { get; init; }
}
