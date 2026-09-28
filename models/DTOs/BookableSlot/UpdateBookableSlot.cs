namespace Reservae.Models.DTOs;

public class UpdateBookableSlotDTO
{
    public DateTime? StartsAt { get; init; }
    public DateTime? EndsAt { get; init; }
    public decimal? CustomPricePerSpot { get; init; }
    public int? Capacity { get; init; }
    public bool? IsActive { get; init; }
}
