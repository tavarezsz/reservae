using Reservae.Models;
using Reservae.Models.DTOs;

namespace Reservae.Service.Mappers;

public static class AvailabilityRuleMapper
{
    public static AvailabilityRuleDto ToDto(this AvailabilityRule entity)
    {
        var spaceName = entity.Space?.Title
            ?? throw new InvalidOperationException(
                "Carregue o Space antes de mapear uma regra de disponibilidade.");

        return new AvailabilityRuleDto
        {
            Id = entity.Id,
            SpaceName = spaceName,
            DayOfTheWeek = entity.DayOfTheWeek,
            StartTime = entity.StartTime,
            EndTime = entity.EndTime,
            ValidFrom = entity.ValidFrom,
            ValidUntil = entity.ValidUntil,
            CustomPricePerSpot = entity.CustomPricePerSpot,
            IsActive = entity.IsActive,
            Capacity = entity.Capacity
        };
    }

    public static AvailabilityRule ToEntity(this CreateAvailabilityRuleDTO dto)
    {
        var entity = new AvailabilityRule(
            dto.SpaceId,
            dto.DayOfTheWeek,
            dto.StartTime,
            dto.EndTime,
            dto.ValidFrom,
            dto.ValidUntil,
            dto.CustomPricePerSpot,
            dto.Capacity);

        if (dto.IsActive is bool isActive)
            entity.SetActive(isActive);

        return entity;
    }

    public static void ApplyUpdate(
        this UpdateAvailabilityRuleDTO dto,
        AvailabilityRule entity)
    {
        entity.ChangeSchedule(
            dto.DayOfTheWeek ?? entity.DayOfTheWeek,
            dto.StartTime ?? entity.StartTime,
            dto.EndTime ?? entity.EndTime,
            dto.ValidFrom ?? entity.ValidFrom,
            dto.ValidUntil ?? entity.ValidUntil);

        entity.ChangePriceAndCapacity(
            dto.CustomPricePerSpot ?? entity.CustomPricePerSpot,
            dto.Capacity ?? entity.Capacity);

        if (dto.IsActive is bool isActive)
            entity.SetActive(isActive);
    }
}
