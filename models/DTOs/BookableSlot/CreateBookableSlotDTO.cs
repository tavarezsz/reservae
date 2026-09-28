using System.ComponentModel.DataAnnotations;

namespace Reservae.Models.DTOs;

public class CreateBookableSlotDTO
{
    [Range(1, int.MaxValue)]
    public int AvailabilityRuleId { get; init; }

    [Range(1, int.MaxValue)]
    public int SpaceId { get; init; }

    public DateTime StartsAt { get; init; }
    public DateTime EndsAt { get; init; }

    [Range(0, double.MaxValue, ErrorMessage = "O preço não pode ser negativo.")]
    public decimal? CustomPricePerSpot { get; init; }

    [Range(1, int.MaxValue)]
    public int Capacity { get; init; }

    public bool? IsActive { get; init; }
}
