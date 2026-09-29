namespace Reservae.Models.DTOs;

public class AvailableSlotDto
{
    public int? BookableSlotId { get; init; }
    public int? AvailabilityRuleId { get; init; }
    public int SpaceId { get; init; }
    public DateTime StartsAt { get; init; }
    public DateTime EndsAt { get; init; }
    public decimal PricePerSpot { get; init; }
    public int Capacity { get; init; }
    public int ReservedQuantity { get; init; }
    public int AvailableQuantity { get; init; }
    public bool IsVirtual { get; init; }
}
