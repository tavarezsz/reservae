using Reservae.Models.Common;

namespace Reservae.Models;

public class Booking : AuditableEntity
{
    public int Id { get; set; }
    public int BookableSlotId { get; set; }
    public BookableSlot BookableSlot { get; set; } = null!;
    public string UserBookedId { get; set; } = string.Empty;
    public User UserBooked { get; set; } = null!;
    public BookingStatusEnum Status { get; set; }
    public int Quantity { get; set; }
}
