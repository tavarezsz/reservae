using Reservae.Models;
using Reservae.Models.DTOs;

namespace Reservae.Models.Interfaces;

public interface IBookableSlotRepository : IBaseRepository<BookableSlot>
{
    Task<PagedResponseDto<BookableSlot>> GetStandaloneForSpaceAsync(int spaceId, int page, int pageSize);
    Task<DateTime?> GetLatestActiveStandaloneStartAsync(
        int spaceId,
        DateTime fromUtc,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BookableSlot>> GetForPeriodAsync(
        int spaceId,
        DateTime periodStart,
        DateTime periodEndExclusive,
        CancellationToken cancellationToken = default);

    Task<BookableSlot?> GetByRuleOccurrenceAsync(
        int availabilityRuleId,
        DateTime startsAt,
        CancellationToken cancellationToken = default);
}
