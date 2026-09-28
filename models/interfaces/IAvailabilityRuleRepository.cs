using Reservae.Models;

namespace Reservae.Models.Interfaces;

public interface IAvailabilityRuleRepository : IBaseRepository<AvailabilityRule>
{
    Task<IReadOnlyList<AvailabilityRule>> GetActiveForPeriodAsync(
        int spaceId,
        DateOnly fromDate,
        DateOnly toDate,
        DayOfTheWeekEnum? dayOfTheWeek = null,
        CancellationToken cancellationToken = default);
}
