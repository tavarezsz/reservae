using Reservae.Models;
using Reservae.Models.DTOs;

namespace Reservae.Models.Interfaces;

public interface IAvailabilityRuleRepository : IBaseRepository<AvailabilityRule>
{
    Task<PagedResponseDto<AvailabilityRule>> GetForSpaceAsync(int spaceId, int page, int pageSize);
    Task<DateTime?> GetLatestActiveValidUntilAsync(
        int spaceId,
        DateTime fromUtc,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AvailabilityRule>> GetActiveForPeriodAsync(
        int spaceId,
        DateOnly fromDate,
        DateOnly toDate,
        DayOfTheWeekEnum? dayOfTheWeek = null,
        CancellationToken cancellationToken = default);
}
