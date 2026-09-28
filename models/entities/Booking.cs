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
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                "Não é possível reservar menos de 1 lugar.");

        if (BookableSlot is null)
            throw new InvalidOperationException(
                "Carregue o BookableSlot antes de alterar a quantidade da reserva.");

        var totalBookings = BookableSlot.GetBookingCount();

        if (totalBookings - Quantity + quantity > BookableSlot.Capacity)
            throw new InvalidOperationException(
                "Não temos espaços disponíveis no horário informado.");

        Quantity = quantity;
    }

}
