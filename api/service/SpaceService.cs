using Microsoft.AspNetCore.Identity;
using Reservae.Models;
using Reservae.Models.DTOs;
using Reservae.Models.Enums;
using Reservae.Models.Interfaces;
using Reservae.Service.Mappers;

namespace Reservae.Service;

public class SpaceService(
    ISpaceRepository spaceRepository,
    IAvailabilityRuleRepository availabilityRuleRepository,
    IBookableSlotRepository bookableSlotRepository,
    UserManager<User> userManager
)
{
    public async Task<PagedResponseDto<SpaceDTO>> SearchAsync(
        string term,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var paged = await spaceRepository.SearchAsync(
            term,
            page,
            pageSize,
            cancellationToken);

        return new PagedResponseDto<SpaceDTO>
        {
            Items = paged.Items.Select(space => space.ToDto()),
            TotalCount = paged.TotalCount
        };
    }

    public async Task<PagedResponseDto<SpaceDTO>> GetPagedAsync(int page, int pagesize)
    {
        var paged = await spaceRepository.GetActivePagedAsync(page, pagesize);

        return new PagedResponseDto<SpaceDTO>
        {
            Items = paged.Items.Select(i => i.ToDto()),
            TotalCount = paged.TotalCount
        };
    }

    public async Task<PagedResponseDto<SpaceDTO>> GetForOwnerAsync(string ownerId, int page, int pageSize)
    {
        var paged = await spaceRepository.GetForOwnerAsync(ownerId, page, pageSize);
        return new PagedResponseDto<SpaceDTO>
        {
            Items = paged.Items.Select(space => space.ToDto()),
            TotalCount = paged.TotalCount
        };
    }

    public async Task<SpaceDTO> GetByIdAsync(int id)
    {
        var space = await spaceRepository.GetByIdAsync(id)
            ?? throw new ResourceNotFoundException($"Espaço com id {id} não encontrada.");
        return space.ToDto();
    }

    public async Task<SpaceDTO> ChangeCoverImageAsync(
        int id,
        string coverImagePath)
    {
        var space = await spaceRepository.GetByIdAsync(id)
            ?? throw new ResourceNotFoundException($"Espaço com id {id} não encontrado.");

        space.ChangeCoverImage(coverImagePath);
        await spaceRepository.UpdateAsync(space);

        return space.ToDto();
    }

    public async Task<SpaceDTO> CreateAsync(CreateSpaceDto dto)
    {
        var owner = await userManager.FindByIdAsync(dto.OwnerId)
            ?? throw new ResourceNotFoundException($"Usuário com id {dto.OwnerId} não encontrado.");

        var space = new Space(
            owner.Id,
            dto.Address,
            dto.Title,
            dto.Description);

        if (dto.Category is CategoryEnum category)
            space.ChangeCategory(category);

        if (dto.PricePerSpot is decimal pricePerSpot)
            space.ChangePrice(pricePerSpot);

        var createdSpace = await spaceRepository.AddAsync(space);
        return createdSpace.ToDto();
    }

    public async Task<SpaceDTO> UpdateAsync(int id, UpdateSpaceDto dto)
    {
        var space = await spaceRepository.GetByIdAsync(id)
            ?? throw new ResourceNotFoundException($"Espaço com id {id} não encontrada.");

        SpaceMapper.ApplyUpdate(dto, space);
        await spaceRepository.UpdateAsync(space);

        return space.ToDto();

    }

    public async Task DeleteAsync(int id)
    {
        throw new NotImplementedException("Nâo implementado");
    }

    public async Task<SpaceAvailabilityLimitDto> GetAvailabilityLimitAsync(
        int spaceId,
        CancellationToken cancellationToken = default)
    {
        _ = await spaceRepository.GetByIdAsync(spaceId)
            ?? throw new ResourceNotFoundException($"Espaço com id {spaceId} não encontrado.");

        var latestRule = await availabilityRuleRepository.GetLatestActiveValidUntilAsync(
            spaceId, DateTime.UtcNow.Date, cancellationToken);
        var latestStandalone = await bookableSlotRepository.GetLatestActiveStandaloneStartAsync(
            spaceId, DateTime.UtcNow, cancellationToken);
        var latest = new[] { latestRule, latestStandalone }.Max();

        return new SpaceAvailabilityLimitDto
        {
            ValidUntil = latest is DateTime value
                ? DateOnly.FromDateTime(value)
                : null
        };
    }

    public async Task<IReadOnlyList<AvailableSlotDto>> GetAvailabilityAsync(
        int spaceId,
        DateOnly fromDate,
        DateOnly toDate,
        DayOfTheWeekEnum? dayOfTheWeek = null,
        CancellationToken cancellationToken = default)
    {
        if (toDate < fromDate)
            throw new ArgumentException(
                "A data final deve ser igual ou posterior à data inicial.",
                nameof(toDate));

        _ = await spaceRepository.GetByIdAsync(spaceId)
            ?? throw new ResourceNotFoundException($"Espaço com id {spaceId} não encontrado.");

        var periodStart = ToUtcDateTime(fromDate, TimeOnly.MinValue);
        var periodEndExclusive = ToUtcDateTime(toDate.AddDays(1), TimeOnly.MinValue);

        var rules = await availabilityRuleRepository.GetActiveForPeriodAsync(
            spaceId,
            fromDate,
            toDate,
            dayOfTheWeek,
            cancellationToken);
        var persistedSlots = await bookableSlotRepository.GetForPeriodAsync(
            spaceId,
            periodStart,
            periodEndExclusive,
            cancellationToken);

        var materializedByOccurrence = persistedSlots
            .Where(slot => slot.AvailabilityRuleId.HasValue)
            .ToDictionary(
                slot => (slot.AvailabilityRuleId!.Value, slot.StartsAt),
                slot => slot);

        var result = new List<AvailableSlotDto>();

        foreach (var rule in rules)
        {
            var firstDate = Max(fromDate, DateOnly.FromDateTime(rule.ValidFrom));
            var lastDate = Min(toDate, DateOnly.FromDateTime(rule.ValidUntil));

            for (var date = firstDate; date <= lastDate; date = date.AddDays(1))
            {
                if (ToRuleDay(date.DayOfWeek) != rule.DayOfTheWeek)
                    continue;

                var slotStartTime = rule.StartTime;
                var duration = TimeSpan.FromMinutes(rule.SlotDurationMinutes);

                while (slotStartTime.Add(duration) <= rule.EndTime)
                {
                    var startsAt = ToUtcDateTime(date, slotStartTime);
                    var endsAt = startsAt.Add(duration);
                    var occurrenceKey = (rule.Id, startsAt);

                    if (materializedByOccurrence.TryGetValue(occurrenceKey, out var persistedSlot))
                    {
                        if (persistedSlot.IsActive)
                            result.Add(ToAvailableSlotDto(persistedSlot));
                    }
                    else
                    {
                        result.Add(new AvailableSlotDto
                        {
                            BookableSlotId = null,
                            AvailabilityRuleId = rule.Id,
                            SpaceId = rule.SpaceId,
                            StartsAt = startsAt,
                            EndsAt = endsAt,
                            PricePerSpot = rule.GetEffectivePricePerSpot(),
                            Capacity = rule.Capacity,
                            ReservedQuantity = 0,
                            AvailableQuantity = rule.Capacity,
                            IsVirtual = true
                        });
                    }

                    slotStartTime = slotStartTime.Add(duration);
                }
            }
        }

        result.AddRange(persistedSlots
            .Where(slot =>
                slot.AvailabilityRuleId is null &&
                slot.IsActive &&
                (dayOfTheWeek is null ||
                 ToRuleDay(slot.StartsAt.DayOfWeek) == dayOfTheWeek))
            .Select(ToAvailableSlotDto));

        return result
            .OrderBy(slot => slot.StartsAt)
            .ThenBy(slot => slot.EndsAt)
            .ToList();
    }

    private static AvailableSlotDto ToAvailableSlotDto(BookableSlot slot)
    {
        var reservedQuantity = slot.Bookings
            .Where(booking => booking.Status == BookingStatusEnum.Confirmado)
            .Sum(booking => booking.Quantity);

        return new AvailableSlotDto
        {
            BookableSlotId = slot.Id,
            AvailabilityRuleId = slot.AvailabilityRuleId,
            SpaceId = slot.SpaceId,
            StartsAt = slot.StartsAt,
            EndsAt = slot.EndsAt,
            PricePerSpot = slot.GetEffectivePricePerSpot(),
            Capacity = slot.Capacity,
            ReservedQuantity = reservedQuantity,
            AvailableQuantity = Math.Max(0, slot.Capacity - reservedQuantity),
            IsVirtual = false
        };
    }

    private static DateTime ToUtcDateTime(DateOnly date, TimeOnly time)
        => DateTime.SpecifyKind(date.ToDateTime(time), DateTimeKind.Utc);

    private static DateOnly Max(DateOnly first, DateOnly second)
        => first >= second ? first : second;

    private static DateOnly Min(DateOnly first, DateOnly second)
        => first <= second ? first : second;

    private static DayOfTheWeekEnum ToRuleDay(DayOfWeek dayOfWeek)
        => dayOfWeek switch
        {
            DayOfWeek.Monday => DayOfTheWeekEnum.Monday,
            DayOfWeek.Tuesday => DayOfTheWeekEnum.Tuesday,
            DayOfWeek.Wednesday => DayOfTheWeekEnum.Wednesday,
            DayOfWeek.Thursday => DayOfTheWeekEnum.Thursday,
            DayOfWeek.Friday => DayOfTheWeekEnum.Friday,
            DayOfWeek.Saturday => DayOfTheWeekEnum.Saturday,
            DayOfWeek.Sunday => DayOfTheWeekEnum.Sunday,
            _ => throw new ArgumentOutOfRangeException(nameof(dayOfWeek))
        };

}
