namespace Reservae.Models.DTOs;

public class CreateBookingDto
{
    public int BookableSlotId { get; set; }
    public BookingStatusEnum Status { get; set; }
    public int Quantity { get; set; }
}
