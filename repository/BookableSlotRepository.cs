using Microsoft.EntityFrameworkCore;
using Reservae.Data;
using Reservae.Models;
using Reservae.Models.DTOs;
using Reservae.Models.Interfaces;
using Reservae.Repository.Extensions;

namespace Reservae.Repository;

public class BookableSlotRepository(ApplicationDbContext context)
    : BaseRepository<BookableSlot>(context), IBookableSlotRepository
{
    public override async Task<BookableSlot?> GetByIdAsync(int id)
        => await DbSet
            .Include(slot => slot.AvailabilityRule)
            .Include(slot => slot.Space)
            .Include(slot => slot.Bookings)
            .FirstOrDefaultAsync(slot => slot.Id == id);

    public override async Task<PagedResponseDto<BookableSlot>> GetPagedAsync(
        int page,
        int pageSize)
        => await DbSet
            .Include(slot => slot.AvailabilityRule)
            .Include(slot => slot.Space)
            .Include(slot => slot.Bookings)
            .OrderBy(slot => slot.StartsAt)
            .ToPagedAsync(page, pageSize);

    public async Task<IReadOnlyList<BookableSlot>> GetForPeriodAsync(
        int spaceId,
        DateTime periodStart,
        DateTime periodEndExclusive,
        CancellationToken cancellationToken = default)
    {
        if (spaceId <= 0)
            throw new ArgumentOutOfRangeException(nameof(spaceId));
        if (periodEndExclusive <= periodStart)
            throw new ArgumentException(
                "O término do período deve ser posterior ao início.",
                nameof(periodEndExclusive));

        return await DbSet
            .AsNoTracking()
            .Include(slot => slot.AvailabilityRule)
            .Include(slot => slot.Space)
            .Include(slot => slot.Bookings)
            .Where(slot =>
                slot.SpaceId == spaceId &&
                slot.StartsAt < periodEndExclusive &&
                slot.EndsAt > periodStart)
            .OrderBy(slot => slot.StartsAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<BookableSlot?> GetByRuleOccurrenceAsync(
        int availabilityRuleId,
        DateTime startsAt,
        CancellationToken cancellationToken = default)
        => await DbSet
            .Include(slot => slot.AvailabilityRule)
            .Include(slot => slot.Space)
            .Include(slot => slot.Bookings)
            .FirstOrDefaultAsync(
                slot =>
                    slot.AvailabilityRuleId == availabilityRuleId &&
                    slot.StartsAt == startsAt,
                cancellationToken);
}
