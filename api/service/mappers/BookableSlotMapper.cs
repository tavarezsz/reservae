using Reservae.Models;
using Reservae.Models.DTOs;

namespace Reservae.Service.Mappers;

public static class BookableSlotMapper
{
    public static BookableSlotDTO ToDto(this BookableSlot entity)
    {
        return new BookableSlotDTO
        {
            Id = entity.Id,
            AvailabilityRuleId = entity.AvailabilityRuleId,
            SpaceId = entity.SpaceId,
            StartsAt = entity.StartsAt,
            EndsAt = entity.EndsAt,
            CustomPricePerSpot = entity.CustomPricePerSpot,
            Capacity = entity.Capacity,
            IsActive = entity.IsActive,
            ReservedQuantity = entity.GetBookingCount()
        };
    }

    public static BookableSlot ToEntity(
        this CreateBookableSlotDTO dto,
        int resolvedSpaceId,
        int resolvedCapacity)
    {
        var entity = dto.AvailabilityRuleId is int availabilityRuleId
            ? BookableSlot.FromRule(
                availabilityRuleId,
                resolvedSpaceId,
                dto.StartsAt,
                dto.EndsAt,
                dto.CustomPricePerSpot,
                resolvedCapacity)
            : BookableSlot.CreateStandalone(
                resolvedSpaceId,
                dto.StartsAt,
                dto.EndsAt,
                dto.CustomPricePerSpot,
                resolvedCapacity);

        if (dto.IsActive is bool isActive)
            entity.SetActive(isActive);

        return entity;
    }

    public static void ApplyUpdate(
        this UpdateBookableSlotDTO dto,
        BookableSlot entity)
    {
        entity.ChangePeriod(
            dto.StartsAt ?? entity.StartsAt,
            dto.EndsAt ?? entity.EndsAt);

        if (dto.CustomPricePerSpot is decimal customPrice)
            entity.ChangeCustomPrice(customPrice);

        if (dto.Capacity is int capacity)
            entity.ChangeCapacity(capacity);

        if (dto.IsActive is bool isActive)
            entity.SetActive(isActive);
    }
}
