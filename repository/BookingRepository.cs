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
}

