using System.Data;
using Reservae.Models;
using Reservae.Models.DTOs;
using Reservae.Models.Interfaces;
using Reservae.Service.Mappers;

namespace Reservae.Service;

public class BookableSlotService(
    IBaseRepository<BookableSlot> bookableSlotRepository,
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
        var rule = await availabilityRepository.GetByIdAsync(dto.AvailabilityRuleId) ?? throw new KeyNotFoundException($"Regra não encontrada");

        var space = await spaceRepository.GetByIdAsync(dto.SpaceId) ?? throw new KeyNotFoundException("Espaço não encontrado");

        var createdSlot = await bookableSlotRepository.AddAsync(dto.ToEntity());
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
        throw new NotImplementedException("Nâo implementado");
    }
}