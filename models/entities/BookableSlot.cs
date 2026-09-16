using Reservae.Models.Common;

namespace Reservae.Models;

public class BookableSlot : AuditableEntity
{
    public int Id { get; set; }
    public int AvailabilityRuleId { get; set; }
    public AvailabilityRule AvailabilityRule { get; set; } = null!;
    public int SpaceId { get; set; }
    public Space Space { get; set; } = null!;
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public decimal CustomPricePerSpot { get; set; }
    public int Capacity { get; set; }
    public bool IsActive { get; set; }
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
