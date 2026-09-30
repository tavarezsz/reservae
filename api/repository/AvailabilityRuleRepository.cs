using Microsoft.EntityFrameworkCore;
using Reservae.Data;
using Reservae.Models;
using Reservae.Models.DTOs;
using Reservae.Models.Interfaces;
using Reservae.Repository.Extensions;

namespace Reservae.Repository;

public class AvailabilityRuleRepository(ApplicationDbContext context)
    : BaseRepository<AvailabilityRule>(context), IAvailabilityRuleRepository
{
    public Task<PagedResponseDto<AvailabilityRule>> GetForSpaceAsync(int spaceId, int page, int pageSize)
        => DbSet.AsNoTracking()
            .Include(rule => rule.Space)
            .Where(rule => rule.SpaceId == spaceId)
            .OrderBy(rule => rule.DayOfTheWeek)
            .ThenBy(rule => rule.StartTime)
            .ToPagedAsync(page, pageSize);

    public Task<DateTime?> GetLatestActiveValidUntilAsync(
        int spaceId,
        DateTime fromUtc,
        CancellationToken cancellationToken = default)
        => DbSet.AsNoTracking()
            .Where(rule => rule.SpaceId == spaceId && rule.IsActive && rule.ValidUntil >= fromUtc)
            .MaxAsync(rule => (DateTime?)rule.ValidUntil, cancellationToken);

    public override async Task<AvailabilityRule?> GetByIdAsync(int id)
        => await DbSet
            .Include(rule => rule.Space)
            .FirstOrDefaultAsync(rule => rule.Id == id);

    public override async Task<PagedResponseDto<AvailabilityRule>> GetPagedAsync(
        int page,
        int pageSize)
        => await DbSet
            .Include(rule => rule.Space)
            .OrderBy(rule => rule.CreatedAt)
            .ToPagedAsync(page, pageSize);

    public async Task<IReadOnlyList<AvailabilityRule>> GetActiveForPeriodAsync(
        int spaceId,
        DateOnly fromDate,
        DateOnly toDate,
        DayOfTheWeekEnum? dayOfTheWeek = null,
        CancellationToken cancellationToken = default)
    {
        if (spaceId <= 0)
            throw new ArgumentOutOfRangeException(nameof(spaceId));
        if (toDate < fromDate)
            throw new ArgumentException(
                "A data final deve ser igual ou posterior à data inicial.",
                nameof(toDate));

        var periodStart = DateTime.SpecifyKind(
            fromDate.ToDateTime(TimeOnly.MinValue),
            DateTimeKind.Utc);
        var periodEnd = DateTime.SpecifyKind(
            toDate.ToDateTime(TimeOnly.MaxValue),
            DateTimeKind.Utc);

        var query = DbSet
            .AsNoTracking()
            .Include(rule => rule.Space)
            .Where(rule =>
                rule.SpaceId == spaceId &&
                rule.IsActive &&
                rule.ValidFrom <= periodEnd &&
                rule.ValidUntil >= periodStart);

        if (dayOfTheWeek is DayOfTheWeekEnum selectedDay)
            query = query.Where(rule => rule.DayOfTheWeek == selectedDay);

        return await query
            .OrderBy(rule => rule.DayOfTheWeek)
            .ThenBy(rule => rule.StartTime)
            .ToListAsync(cancellationToken);
    }
}
