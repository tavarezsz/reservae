using Reservae.Models.Common;
using Reservae.Models.Enums;

namespace Reservae.Models;

public class Space : AuditableEntity
{
    private Space() { }

    public Space(
        string ownerId,
        string address,
        string title,
        string description,
        string coverImagePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(address);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentException.ThrowIfNullOrWhiteSpace(coverImagePath);

        OwnerId = ownerId;
        Address = address;
        Title = title;
        Description = description;
        CoverImagePath = coverImagePath;
        IsActive = true;
    }

    public string OwnerId { get; private set; } = string.Empty;
    public User Owner { get; private set; } = null!;
    public string Address { get; set; } = string.Empty;
    public decimal PricePerSpot { get; private set; }
    public bool IsActive { get; private set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public CategoryEnum Category { get; set; }
    public string? CoverImagePath { get; private set; }
    public ICollection<AvailabilityRule> AvailabilityRules { get; private set; } = new List<AvailabilityRule>();
    public ICollection<BookableSlot> BookableSlots { get; private set; } = new List<BookableSlot>();

    public void ChangePrice(decimal pricePerSpot)
    {
        if (pricePerSpot < 0)
            throw new ArgumentOutOfRangeException(nameof(pricePerSpot), "O preço não pode ser negativo.");

        PricePerSpot = pricePerSpot;
    }

    public void ChangeCategory(CategoryEnum category) => Category = category;

    public void ChangeCoverImage(string coverImagePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(coverImagePath);
        CoverImagePath = coverImagePath;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
    public void SetActive(bool isActive) => IsActive = isActive;
}
