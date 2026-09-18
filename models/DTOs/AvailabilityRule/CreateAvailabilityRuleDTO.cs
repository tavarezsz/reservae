using System.ComponentModel.DataAnnotations;

namespace Reservae.Models.DTOs;

public class CreateAvailabilityRuleDTO
{
    [Range(1, int.MaxValue)]
    public int SpaceId { get; init; }

    public DayOfTheWeekEnum DayOfTheWeek { get; init; }
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
    public DateTime ValidFrom { get; init; }
    public DateTime ValidUntil { get; init; }

    [Range(0, double.MaxValue, ErrorMessage = "O preço não pode ser negativo.")]
    public decimal? CustomPricePerSpot { get; init; }

    public bool? IsActive { get; init; }

    [Range(1, int.MaxValue)]
    public int Capacity { get; init; }
}
