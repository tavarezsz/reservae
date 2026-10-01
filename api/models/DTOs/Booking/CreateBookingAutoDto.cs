using System.ComponentModel.DataAnnotations;

namespace Reservae.Models.DTOs;

public class CreateBookingAutoDto
{
    [Range(1, int.MaxValue)]
    public int? BookableSlotId { get; set; }

    [Range(1, int.MaxValue)]
    public int? AvailabilityRuleId { get; set; }

    [Range(1, int.MaxValue)]
    public int SpaceId { get; set; }

    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

}
