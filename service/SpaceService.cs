using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Reservae.Models;
using Reservae.Models.DTOs;
using Reservae.Models.Interfaces;
using Reservae.Modesl.DTOs;
using Reservae.Service.Mappers;
using System.Linq;

namespace Reservae.Service;

public class SpaceService(
    IBaseRepository<Space> spaceRepository,
    UserManager<User> userManager
)
{
    public async Task<PagedResponseDto<SpaceDTO>> GetPagedAsync(int page, int pagesize)
    {
        var paged = await spaceRepository.GetPagedAsync(page, pagesize);

        var activeSpaces = paged.Items.Where(i => i.IsActive).ToList();

        return new PagedResponseDto<SpaceDTO>
        {
            Items = activeSpaces.Select(i => i.ToDto()),
            TotalCount = activeSpaces.Count
        };
    }

    public async Task<SpaceDTO> GetByIdAsync(int id)
    {
        var space = await spaceRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Espaço com id {id} não encontrada.");
        return space.ToDto();
    }

    public async Task<SpaceDTO> CreateAsync(Space space)
    {
        var createdSpace = await spaceRepository.AddAsync(space);
        return createdSpace.ToDto();
    }

    public async Task<SpaceDTO> UpdateAsync(int id, UpdateSpaceDto dto)
    {
        var space = await spaceRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Espaço com id {id} não encontrada.");

        SpaceMapper.ApplyUpdate(dto, space);

        return space.ToDto();

    }

    public async Task DeleteAsync(int id)
    {
        throw new NotImplementedException("Nâo implementado");
    }

}
