using Reservae.Models;

namespace Reservae.Models.Interfaces;

public interface IBookableSlotRepository : IBaseRepository<BookableSlot>
{
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
