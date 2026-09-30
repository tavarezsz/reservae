using Reservae.Models.Enums;

namespace Reservae.Models.DTOs;

public class BookingDto
{
    public int Id { get; set; }
    public int BookableSlotId { get; set; }
    public required string UserBookedName { get; set; }
    public BookingStatusEnum Status { get; set; }
    public int Quantity { get; set; }
    public int? SpaceId { get; set; }
    public string? SpaceTitle { get; set; }
    public string? SpaceAddress { get; set; }
    public CategoryEnum? SpaceCategory { get; set; }
    public string? SpaceCoverImagePath { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
}
