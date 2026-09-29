namespace Reservae.Models.DTOs;

public class UpdateBookingDto
{
    public BookingStatusEnum? Status { get; set; }
    public int? Quantity { get; set; }
}
