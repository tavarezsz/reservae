using Reservae.Models;
using Reservae.Models.DTOs;
using Reservae.Models.Interfaces;
using Reservae.Service.Mappers;

namespace Reservae.Service;

public class BookableSlotService(
    IBookableSlotRepository bookableSlotRepository,
    IBaseRepository<AvailabilityRule> availabilityRepository,
    IBaseRepository<Space> spaceRepository

)
{
    public async Task<PagedResponseDto<BookableSlotDTO>> GetPagedAsync(int page, int pageSize)
    {
        var paged = await bookableSlotRepository.GetPagedAsync(page, pageSize);
        return new PagedResponseDto<BookableSlotDTO>
        {
            Items = paged.Items.Select(slot => slot.ToDto()),
            TotalCount = paged.TotalCount
        };
    }

    public async Task<BookableSlotDTO> GetByIdAsync(int id)
    {
        var slot = await bookableSlotRepository.GetByIdAsync(id) ?? throw new KeyNotFoundException("Horário não encontrado");
        return slot.ToDto();
    }

    public async Task<BookableSlotDTO> CreateAsync(CreateBookableSlotDTO dto)
    {
        AvailabilityRule? rule = null;
        int spaceId;
        int capacity;

        if (dto.AvailabilityRuleId is int availabilityRuleId)
        {
            rule = await availabilityRepository.GetByIdAsync(availabilityRuleId)
                ?? throw new KeyNotFoundException("Regra não encontrada.");

            ValidateOccurrenceAgainstRule(rule, dto.StartsAt, dto.EndsAt);

            if (dto.SpaceId is int requestedSpaceId && requestedSpaceId != rule.SpaceId)
                throw new ArgumentException("O espaço informado não pertence à regra de disponibilidade.");

            spaceId = rule.SpaceId;
            capacity = dto.Capacity ?? rule.Capacity;
        }
        else
        {
            spaceId = dto.SpaceId
                ?? throw new ArgumentException("SpaceId é obrigatório para um evento avulso.");
            capacity = dto.Capacity
                ?? throw new ArgumentException("Capacity é obrigatória para um evento avulso.");
        }

        _ = await spaceRepository.GetByIdAsync(spaceId)
            ?? throw new KeyNotFoundException("Espaço não encontrado.");

        var createdSlot = await bookableSlotRepository.AddAsync(
            dto.ToEntity(spaceId, capacity));
        return createdSlot.ToDto();

    }

    public async Task<BookableSlotDTO> UpdateAsync(int id, UpdateBookableSlotDTO dto)
    {
        var slot = await bookableSlotRepository.GetByIdAsync(id) ?? throw new KeyNotFoundException("Horário não encontrado");

        if (slot.AvailabilityRuleId.HasValue &&
            (dto.StartsAt.HasValue || dto.EndsAt.HasValue))
        {
            var rule = slot.AvailabilityRule
                ?? throw new InvalidOperationException(
                    "Carregue a regra de disponibilidade antes de alterar o período do horário.");

            ValidateOccurrenceAgainstRule(
                rule,
                dto.StartsAt ?? slot.StartsAt,
                dto.EndsAt ?? slot.EndsAt);
        }

        BookableSlotMapper.ApplyUpdate(dto, slot);
        await bookableSlotRepository.UpdateAsync(slot);
        return slot.ToDto();
    }

    public async Task DeleteAsync(int id)
    {
        var slot = await bookableSlotRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Horário não encontrado.");

        if (slot.Bookings.Count > 0)
            throw new InvalidOperationException(
                "Não é possível excluir um horário que possui reservas.");

        await bookableSlotRepository.DeleteAsync(id);
    }

    private static void ValidateOccurrenceAgainstRule(
        AvailabilityRule rule,
        DateTime startsAt,
        DateTime endsAt)
    {
        if (!rule.IsActive)
            throw new InvalidOperationException(
                "Não é possível criar ou reagendar um horário para uma regra inativa.");

        if (startsAt.Kind != DateTimeKind.Utc || endsAt.Kind != DateTimeKind.Utc)
            throw new ArgumentException(
                "StartsAt e EndsAt devem estar em UTC. Use o sufixo Z nas datas.");

        if (endsAt <= startsAt)
            throw new ArgumentException("O término deve ser posterior ao início.");

        var occurrenceDate = DateOnly.FromDateTime(startsAt);
        var endDate = DateOnly.FromDateTime(endsAt);
        var validFrom = DateOnly.FromDateTime(rule.ValidFrom);
        var validUntil = DateOnly.FromDateTime(rule.ValidUntil);

        if (occurrenceDate < validFrom || occurrenceDate > validUntil)
            throw new ArgumentException(
                "A data do horário está fora do período de vigência da regra.");

        if (ToRuleDay(startsAt.DayOfWeek) != rule.DayOfTheWeek)
            throw new ArgumentException(
                "O dia da semana do horário não corresponde ao definido pela regra.");

        if (endDate != occurrenceDate)
            throw new ArgumentException(
                "Um horário vinculado a uma regra deve começar e terminar no mesmo dia.");

        var expectedDuration = TimeSpan.FromMinutes(rule.SlotDurationMinutes);
        if (endsAt - startsAt != expectedDuration)
            throw new ArgumentException(
                $"O horário deve ter exatamente {rule.SlotDurationMinutes} minutos.");

        var startTime = TimeOnly.FromDateTime(startsAt);
        var endTime = TimeOnly.FromDateTime(endsAt);

        if (startTime < rule.StartTime || endTime > rule.EndTime)
            throw new ArgumentException(
                "O horário deve estar dentro do intervalo definido pela regra.");

        var offsetFromRuleStart = startTime - rule.StartTime;
        if (offsetFromRuleStart.Ticks % expectedDuration.Ticks != 0)
            throw new ArgumentException(
                "O início do horário deve coincidir com um dos intervalos gerados pela regra.");
    }

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
