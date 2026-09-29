using Reservae.Models.Enums;

namespace Reservae.Models.DTOs;

public class UpdateSpaceDto
{ 
    public string? Address { get; set; }
    public decimal? PricePerSpot { get; set; }
    public bool? IsActive { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public CategoryEnum? Category { get; set; }
    public string? CoverImagePath { get; set; }
}