namespace Reservae.Models.DTOs;

public class BookingDto
{
    public int Id { get; set; }
    public int BookableSlotId { get; set; }
    public required string UserBookedName { get; set; }
    public BookingStatusEnum Status { get; set; }
    public int Quantity { get; set; }
}
