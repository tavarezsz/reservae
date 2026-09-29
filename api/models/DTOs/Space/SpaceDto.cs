using Reservae.Models.Enums;

namespace Reservae.Models.DTOs;


public class SpaceDTO
{
    public int Id { get; set; }
    public required string OwnerId { get; set; }
    public required string Address { get; set; }
    public decimal PricePerSpot { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public CategoryEnum Category { get; set; }
    public string? CoverImagePath { get; set; }
}
