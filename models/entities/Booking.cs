using Reservae.Models.Common;

namespace Reservae.Models;

public class Booking : AuditableEntity
{
    private Booking() { }

    public Booking(
        int bookableSlotId,
        string userBookedId,
        BookingStatusEnum status,
        int quantity)
    {
        if (bookableSlotId <= 0)
            throw new ArgumentOutOfRangeException(nameof(bookableSlotId));
        ArgumentException.ThrowIfNullOrWhiteSpace(userBookedId);
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "A quantidade deve ser positiva.");

        BookableSlotId = bookableSlotId;
        UserBookedId = userBookedId;
        Status = status;
        Quantity = quantity;
    }

    public int BookableSlotId { get; private set; }
    public BookableSlot BookableSlot { get; private set; } = null!;
    public string UserBookedId { get; private set; } = string.Empty;
    public User UserBooked { get; private set; } = null!;
    public BookingStatusEnum Status { get; set; }
    public int Quantity { get; private set; }

    public void ChangeQuantity(int quantity)
    {
        if(quantity <= 0)
        {
            throw new ArgumentOutOfRangeException("Não é possível revervar menos de 1 lugar");
        } 

        int TotalBookings = BookableSlot.GetBookingCount();

        if(TotalBookings - Quantity + quantity > BookableSlot.Capacity)
        {
            throw new Exception("Não temos espaços disponíveis no horário informado");
        }

    }

}
