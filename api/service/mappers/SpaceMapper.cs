using Reservae.Models;
using Reservae.Models.DTOs;

namespace Reservae.Service.Mappers;

public static class SpaceMapper
{
    public static SpaceDTO ToDto(this Space entity)
    {
        return new SpaceDTO
        {
            Id = entity.Id,
            OwnerId = entity.OwnerId,
            Address = entity.Address,
            PricePerSpot = entity.PricePerSpot,
            Title = entity.Title,
            Description = entity.Description,
            Category = entity.Category,
            CoverImagePath = entity.CoverImagePath
        };
    }

    public static void ApplyUpdate(this UpdateSpaceDto dto, Space entity)
    {
        entity.Address = dto.Address ?? entity.Address;
        entity.Title = dto.Title ?? entity.Title;
        entity.Description = dto.Description ?? entity.Description;
        entity.Category = dto.Category ?? entity.Category;

        if (dto.PricePerSpot is decimal pricePerSpot)
            entity.ChangePrice(pricePerSpot);

        if (dto.IsActive is bool isActive)
            entity.SetActive(isActive);

    }
}
