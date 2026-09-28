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
}
