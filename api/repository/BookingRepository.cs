using Microsoft.EntityFrameworkCore;
using Reservae.Data;
using Reservae.Models;
using Reservae.Models.DTOs;
using Reservae.Models.Interfaces;
using Reservae.Repository.Extensions;

namespace Reservae.Repository;

public class BookingRepository(ApplicationDbContext context)
    : BaseRepository<Booking>(context), IBookingRepository
{
    public override async Task<Booking?> GetByIdAsync(int id)
        => await DbSet
            .Include(booking => booking.UserBooked)
            .Include(booking => booking.BookableSlot)
                .ThenInclude(slot => slot.Bookings)
            .FirstOrDefaultAsync(booking => booking.Id == id);

    public override async Task<Booking> AddAsync(Booking booking)
    {
        await using var transaction = await Context.Database.BeginTransactionAsync();

        var slot = await LockBookableSlotAsync(booking.BookableSlotId);
        await ValidateSlotCanReceiveBookingAsync(slot, booking.Quantity, booking.Status);

        await DbSet.AddAsync(booking);
        await Context.SaveChangesAsync();
        await transaction.CommitAsync();

        return booking;
    }

    public override async Task UpdateAsync(Booking booking)
    {
        await using var transaction = await Context.Database.BeginTransactionAsync();

        var slot = await LockBookableSlotAsync(booking.BookableSlotId);
        var reservedByOtherBookings = await DbSet
            .Where(current =>
                current.BookableSlotId == booking.BookableSlotId &&
                current.Id != booking.Id &&
                current.Status == BookingStatusEnum.Confirmado)
            .SumAsync(current => (int?)current.Quantity) ?? 0;
        var requestedQuantity = booking.Status == BookingStatusEnum.Confirmado
            ? booking.Quantity
            : 0;

        if (reservedByOtherBookings + requestedQuantity > slot.Capacity)
            throw new InvalidOperationException(
                "Não temos espaços disponíveis no horário informado.");

        booking.UpdatedAt = DateTime.UtcNow;
        DbSet.Update(booking);
        await Context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task<PagedResponseDto<Booking>> GetBySpaceIdAsync(
        int spaceId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (spaceId <= 0)
            throw new ArgumentOutOfRangeException(nameof(spaceId));

        return await DbSet
            .AsNoTracking()
            .Include(booking => booking.UserBooked)
            .Include(booking => booking.BookableSlot)
            .Where(booking => booking.BookableSlot.SpaceId == spaceId)
            .OrderBy(booking => booking.BookableSlot.StartsAt)
            .ToPagedAsync(page, pageSize, cancellationToken);
    }

    public async Task<PagedResponseDto<Booking>> GetByUserIdAsync(
        string userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        return await DbSet
            .AsNoTracking()
            .Include(booking => booking.UserBooked)
            .Include(booking => booking.BookableSlot)
            .Where(booking => booking.UserBookedId == userId)
            .OrderBy(booking => booking.BookableSlot.StartsAt)
            .ToPagedAsync(page, pageSize, cancellationToken);
    }

    private async Task<BookableSlot> LockBookableSlotAsync(int bookableSlotId)
        => await Context.BookableSlots
            .FromSqlInterpolated(
                $"""SELECT * FROM "BookableSlots" WHERE "Id" = {bookableSlotId} FOR UPDATE""")
            .SingleOrDefaultAsync()
            ?? throw new ResourceNotFoundException("Horário não encontrado.");

    private async Task ValidateSlotCanReceiveBookingAsync(
        BookableSlot slot,
        int quantity,
        BookingStatusEnum status)
    {
        if (!slot.IsActive)
            throw new InvalidOperationException("Não é possível reservar um horário inativo.");

        if (status != BookingStatusEnum.Confirmado)
            return;

        var reservedQuantity = await DbSet
            .Where(booking =>
                booking.BookableSlotId == slot.Id &&
                booking.Status == BookingStatusEnum.Confirmado)
            .SumAsync(booking => (int?)booking.Quantity) ?? 0;

        if (reservedQuantity + quantity > slot.Capacity)
            throw new InvalidOperationException(
                "Não temos espaços disponíveis no horário informado.");
    }
}

