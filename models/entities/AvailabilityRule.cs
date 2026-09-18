
using Reservae.Models.Common;

namespace Reservae.Models
{
    public class AvailabilityRule : AuditableEntity
    {
        private AvailabilityRule() { }

        public AvailabilityRule(
            int spaceId,
            DayOfTheWeekEnum dayOfTheWeek,
            TimeOnly startTime,
            TimeOnly endTime,
            DateTime validFrom,
            DateTime validUntil,
            decimal? customPricePerSpot,
            int capacity)
        {
            ValidatePeriod(startTime, endTime, validFrom, validUntil);
            ValidatePriceAndCapacity(customPricePerSpot, capacity);

            if (spaceId <= 0)
                throw new ArgumentOutOfRangeException(nameof(spaceId));

            SpaceId = spaceId;
            DayOfTheWeek = dayOfTheWeek;
            StartTime = startTime;
            EndTime = endTime;
            ValidFrom = validFrom;
            ValidUntil = validUntil;
            CustomPricePerSpot = customPricePerSpot;
            Capacity = capacity;
            IsActive = true;
        }

        public int SpaceId { get; private set; }
        public Space Space { get; private set; } = null!;
        public DayOfTheWeekEnum DayOfTheWeek { get; private set; }
        public TimeOnly StartTime { get; private set; }
        public TimeOnly EndTime { get; private set; }
        public DateTime ValidFrom { get; private set; }
        public DateTime ValidUntil { get; private set; }
        public decimal? CustomPricePerSpot { get; private set; }
        public bool IsActive { get; private set; }
        public int Capacity { get; private set; }
        public ICollection<BookableSlot> BookableSlots { get; private set; } = new List<BookableSlot>();

        public void ChangePriceAndCapacity(decimal? customPricePerSpot, int capacity)
        {
            ValidatePriceAndCapacity(customPricePerSpot, capacity);
            CustomPricePerSpot = customPricePerSpot;
            Capacity = capacity;
        }

        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;
        public void SetActive(bool isActive) => IsActive = isActive;

        public void ChangeSchedule(
            DayOfTheWeekEnum dayOfTheWeek,
            TimeOnly startTime,
            TimeOnly endTime,
            DateTime validFrom,
            DateTime validUntil)
        {
            ValidatePeriod(startTime, endTime, validFrom, validUntil);

            DayOfTheWeek = dayOfTheWeek;
            StartTime = startTime;
            EndTime = endTime;
            ValidFrom = validFrom;
            ValidUntil = validUntil;
        }

        private static void ValidatePeriod(
            TimeOnly startTime,
            TimeOnly endTime,
            DateTime validFrom,
            DateTime validUntil)
        {
            if (endTime <= startTime)
                throw new ArgumentException("O horário final deve ser posterior ao horário inicial.");
            if (validUntil < validFrom)
                throw new ArgumentException("A validade final deve ser posterior à validade inicial.");
        }

        private static void ValidatePriceAndCapacity(decimal? price, int capacity)
        {
            if (price is < 0)
                throw new ArgumentOutOfRangeException(nameof(price), "O preço não pode ser negativo.");
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "A capacidade deve ser positiva.");
        }

        public void ChangeCustomPrice(decimal? price)
        {
            if (price is < 0)
                throw new ArgumentOutOfRangeException(nameof(price), "O preço não pode ser negativo.");

            CustomPricePerSpot = price;
        }

        public decimal GetEffectivePricePerSpot()
        {
            if (CustomPricePerSpot is decimal customPrice)
                return customPrice;

            if (Space is null)
                throw new InvalidOperationException("Carregue o Space para calcular o preço efetivo da regra.");

            return Space.PricePerSpot;
        }
    }
}
