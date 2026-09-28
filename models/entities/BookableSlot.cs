using Reservae.Models.Common;

namespace Reservae.Models;

public class BookableSlot : AuditableEntity
{
    private BookableSlot() { }

    private BookableSlot(
        int? availabilityRuleId,
        int spaceId,
        DateTime startsAt,
        DateTime endsAt,
        decimal? customPricePerSpot,
        int capacity)
    {
        if (availabilityRuleId is <= 0)
            throw new ArgumentOutOfRangeException(nameof(availabilityRuleId));
        if (spaceId <= 0)
            throw new ArgumentOutOfRangeException(nameof(spaceId));
        if (endsAt <= startsAt)
            throw new ArgumentException("O término deve ser posterior ao início.");
        if (customPricePerSpot is < 0)
            throw new ArgumentOutOfRangeException(nameof(customPricePerSpot), "O preço não pode ser negativo.");
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), "A capacidade deve ser positiva.");

        AvailabilityRuleId = availabilityRuleId;
        SpaceId = spaceId;
        StartsAt = startsAt;
        EndsAt = endsAt;
        CustomPricePerSpot = customPricePerSpot;
        Capacity = capacity;
        IsActive = true;
    }

    public static BookableSlot FromRule(
        int availabilityRuleId,
        int spaceId,
        DateTime startsAt,
        DateTime endsAt,
        decimal? customPricePerSpot,
        int capacity)
    {
        return new BookableSlot(
            availabilityRuleId,
            spaceId,
            startsAt,
            endsAt,
            customPricePerSpot,
            capacity);
    }

    public static BookableSlot CreateStandalone(
        int spaceId,
        DateTime startsAt,
        DateTime endsAt,
        decimal? customPricePerSpot,
        int capacity)
    {
        return new BookableSlot(
            null,
            spaceId,
            startsAt,
            endsAt,
            customPricePerSpot,
            capacity);
    }

    public int? AvailabilityRuleId { get; private set; }
    public AvailabilityRule? AvailabilityRule { get; private set; }
    public int SpaceId { get; private set; }
    public Space Space { get; private set; } = null!;
    public DateTime StartsAt { get; private set; } //para eventos não recorrentes
    public DateTime EndsAt { get; private set; } //para eventos não recorrentes
    public decimal? CustomPricePerSpot { get; private set; }
    public int Capacity { get; private set; }
    public bool IsActive { get; private set; }
    public ICollection<Booking> Bookings { get; private set; } = new List<Booking>();

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
    public void SetActive(bool isActive) => IsActive = isActive;

    public void ChangePeriod(DateTime startsAt, DateTime endsAt)
    {
        if (endsAt <= startsAt)
            throw new ArgumentException("O término deve ser posterior ao início.");

        StartsAt = startsAt;
        EndsAt = endsAt;
    }

    public void ChangeCustomPrice(decimal? price)
    {
        if (price is < 0)
            throw new ArgumentOutOfRangeException(nameof(price), "O preço não pode ser negativo.");

        CustomPricePerSpot = price;
    }

    public decimal GetEffectivePricePerSpot()
    {
        if (CustomPricePerSpot is decimal slotPrice)
            return slotPrice;

        if (AvailabilityRule?.CustomPricePerSpot is decimal rulePrice)
            return rulePrice;

        if (Space is null)
            throw new InvalidOperationException(
                "Carregue o Space para calcular o preço efetivo do horário.");

        return Space.PricePerSpot;
    }

    public void ChangeCapacity(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), "A capacidade deve ser positiva.");

        if (Bookings.Count > 0)
            throw new InvalidOperationException(
                "Não é possível mudar a capacidade de um horário com reservas.");

        Capacity = capacity;

    }

    public int GetBookingCount()
    {
        int count = 0;

        foreach(Booking booking in Bookings)
        {
            count += booking.Quantity;
        }

        return count;
    }
}
