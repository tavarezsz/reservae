namespace Reservae.Data.Seed;

public sealed record DemoSeedData(
    int Version,
    DateTime ExportedAt,
    IReadOnlyList<SeedUser> Users,
    IReadOnlyList<SeedSpace> Spaces,
    IReadOnlyList<SeedRule> AvailabilityRules,
    IReadOnlyList<SeedSlot> BookableSlots,
    IReadOnlyList<SeedBooking> Bookings);

// Only public account information is exported. Password hashes, tokens and security stamps are excluded.
public sealed record SeedUser(string Id, string? Name, string Email, string UserName,
    bool EmailConfirmed, DateTime CreatedAt, DateTime UpdatedAt);

public sealed record SeedSpace(int Id, string OwnerId, string Address, decimal PricePerSpot,
    bool IsActive, string Title, string Description, int Category, string? CoverImagePath,
    DateTime CreatedAt, DateTime UpdatedAt);

public sealed record SeedRule(int Id, int SpaceId, int DayOfTheWeek, TimeOnly StartTime,
    TimeOnly EndTime, DateTime ValidFrom, DateTime ValidUntil, decimal? CustomPricePerSpot,
    bool IsActive, int Capacity, int SlotDurationMinutes, DateTime CreatedAt, DateTime UpdatedAt);

public sealed record SeedSlot(int Id, int? AvailabilityRuleId, int SpaceId, DateTime StartsAt,
    DateTime EndsAt, decimal? CustomPricePerSpot, int Capacity, bool IsActive,
    DateTime CreatedAt, DateTime UpdatedAt);

public sealed record SeedBooking(int Id, int BookableSlotId, string UserBookedId, int Status,
    int Quantity, DateTime CreatedAt, DateTime UpdatedAt);
