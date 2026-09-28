namespace Reservae.Models.DTOs;

public class BookableSlotDTO
{
    public int Id { get; set; }
    public int AvailabilityRuleId { get; set; }
    public int SpaceId { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public decimal? CustomPricePerSpot { get; set; }
    public int Capacity { get; set; }
    public bool IsActive { get; set; }
    public int ReservedQuantity { get; set; }
}
