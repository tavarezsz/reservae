using Reservae.Models;
using Reservae.Models.DTOs;
using Reservae.Models.Interfaces;
using Reservae.Service.Mappers;

namespace Reservae.Service;

public class AvailabilityRuleService(
    IAvailabilityRuleRepository availabilityRepository,
    IBaseRepository<Space> spaceRepository
)
{
    public async Task<PagedResponseDto<AvailabilityRuleDto>> GetPagedAsync(int page, int pageSize)
    {
        var paged = await availabilityRepository.GetPagedAsync(page, pageSize);

        return new PagedResponseDto<AvailabilityRuleDto>
        {
            Items = paged.Items.Select(rule => rule.ToDto()),
            TotalCount = paged.TotalCount
        };
    }

    public async Task<AvailabilityRuleDto> GetByIdAsync(int id)
    {
        var rule = await availabilityRepository.GetByIdAsync(id)
            ?? throw new ResourceNotFoundException($"Regra com id {id} não encontrada.");

        return rule.ToDto();
    }

    public async Task<PagedResponseDto<AvailabilityRuleDto>> GetForSpaceAsync(int spaceId, int page, int pageSize)
    {
        var paged = await availabilityRepository.GetForSpaceAsync(spaceId, page, pageSize);
        return new PagedResponseDto<AvailabilityRuleDto>
        {
            Items = paged.Items.Select(rule => rule.ToDto()),
            TotalCount = paged.TotalCount
        };
    }

    public async Task<AvailabilityRuleDto> CreateAsync(CreateAvailabilityRuleDTO dto)
    {
        var space = await spaceRepository.GetByIdAsync(dto.SpaceId)
        ?? throw new ResourceNotFoundException($"Spaço com id {dto.SpaceId} não encontrado");

        var rule = dto.ToEntity();

        var createdRule = await availabilityRepository.AddAsync(rule);
        return createdRule.ToDto();

    }

    public async Task<AvailabilityRuleDto> UpdateAsync(int id, UpdateAvailabilityRuleDTO dto)
    {
        var rule = await availabilityRepository.GetByIdAsync(id)
            ?? throw new ResourceNotFoundException("Regra não encontrada");

        AvailabilityRuleMapper.ApplyUpdate(dto, rule);
        await availabilityRepository.UpdateAsync(rule);

        return rule.ToDto();
    }

    public async Task DeleteAsync(int id)
    {
        throw new NotImplementedException("Nâo implementado");
    }
}
