using Reservae.Models;
using Reservae.Models.DTOs;

namespace Reservae.Service.Mappers;

public static class BookingMapper
{
    public static BookingDto ToDto(this Booking entity)
    {
        var userBookedName = entity.UserBooked?.Name
            ?? entity.UserBooked?.Email
            ?? entity.UserBookedId;

        return new BookingDto
        {
            Id = entity.Id,
            BookableSlotId = entity.BookableSlotId,
            UserBookedName = userBookedName,
            Status = entity.Status,
            Quantity = entity.Quantity
        };
    }

    public static Booking ToEntity(this CreateBookingDto dto)
        => new(
            dto.BookableSlotId,
            dto.UserBookedId,
            dto.Status,
            dto.Quantity);

    public static void ApplyUpdate(this UpdateBookingDto dto, Booking entity)
    {
        if (dto.Status is BookingStatusEnum status)
            entity.ChangeStatus(status);

        if (dto.Quantity is int quantity)
            entity.ChangeQuantity(quantity);
    }
}
