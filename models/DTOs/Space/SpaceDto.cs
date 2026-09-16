using Reservae.Models.Enums;

namespace Reservae.Models.DTOs;


public class SpaceDTO
{
    public int Id { get; set; }
    public string OwnerId { get; set; }
    public string Address { get; set; }
    public decimal PricePerSpot { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public CategoryEnum Category { get; set; }
}