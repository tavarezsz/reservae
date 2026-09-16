using Reservae.Models.Enums;

namespace Reservae.Models;

public class Space
{
    public int Id { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public User Owner { get; set; } = null!;
    public string Address { get; set; } = string.Empty;
    public decimal PricePerSpot { get; set; }
    public bool IsActive { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public CategoryEnum Category { get; set; }
    public string? CoverImagePath { get; set; }
    public ICollection<AvailabilityRule> AvailabilityRules { get; set; } = new List<AvailabilityRule>();
    public ICollection<BookableSlot> BookableSlots { get; set; } = new List<BookableSlot>();
}
