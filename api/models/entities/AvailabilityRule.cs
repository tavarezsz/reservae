
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
            int capacity,
            int slotDurationMinutes)
        {
            ValidatePeriod(startTime, endTime, validFrom, validUntil);
            ValidatePriceAndCapacity(customPricePerSpot, capacity);
            ValidateSlotDuration(slotDurationMinutes, startTime, endTime);

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
            SlotDurationMinutes = slotDurationMinutes;
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
        public int SlotDurationMinutes { get; private set; }
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

        public void ChangeSlotDuration(int slotDurationMinutes)
        {
            ValidateSlotDuration(slotDurationMinutes, StartTime, EndTime);
            SlotDurationMinutes = slotDurationMinutes;
        }

        public void ChangeSchedule(
            DayOfTheWeekEnum dayOfTheWeek,
            TimeOnly startTime,
            TimeOnly endTime,
            DateTime validFrom,
            DateTime validUntil,
            int? slotDurationMinutes = null)
        {
            ValidatePeriod(startTime, endTime, validFrom, validUntil);
            var duration = slotDurationMinutes ?? SlotDurationMinutes;
            ValidateSlotDuration(duration, startTime, endTime);

            DayOfTheWeek = dayOfTheWeek;
            StartTime = startTime;
            EndTime = endTime;
            ValidFrom = validFrom;
            ValidUntil = validUntil;
            SlotDurationMinutes = duration;
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

        private static void ValidateSlotDuration(int slotDurationMinutes, TimeOnly startTime, TimeOnly endTime)
        {
            if (slotDurationMinutes < 30)
                throw new ArgumentOutOfRangeException(
                    nameof(slotDurationMinutes),
                    "A duração mínima de um horário é de 30 minutos.");
            if (TimeSpan.FromMinutes(slotDurationMinutes) > endTime - startTime)
                throw new ArgumentOutOfRangeException(
                    nameof(slotDurationMinutes),
                    "A duração de um horário não pode ser maior que o intervalo entre o início e o fim da regra.");
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

        public void ValidateOccurrence(DateTime startsAt, DateTime endsAt)
        {
            if (!IsActive)
                throw new InvalidOperationException(
                    "Não é possível criar ou reagendar um horário para uma regra inativa.");

            if (startsAt.Kind != DateTimeKind.Utc || endsAt.Kind != DateTimeKind.Utc)
                throw new ArgumentException(
                    "StartsAt e EndsAt devem estar em UTC. Use o sufixo Z nas datas.");

            if (endsAt <= startsAt)
                throw new ArgumentException("O término deve ser posterior ao início.");

            var occurrenceDate = DateOnly.FromDateTime(startsAt);
            var endDate = DateOnly.FromDateTime(endsAt);
            var firstValidDate = DateOnly.FromDateTime(ValidFrom);
            var lastValidDate = DateOnly.FromDateTime(ValidUntil);

            if (occurrenceDate < firstValidDate || occurrenceDate > lastValidDate)
                throw new ArgumentException(
                    "A data do horário está fora do período de vigência da regra.");

            if (ToRuleDay(startsAt.DayOfWeek) != DayOfTheWeek)
                throw new ArgumentException(
                    "O dia da semana do horário não corresponde ao definido pela regra.");

            if (endDate != occurrenceDate)
                throw new ArgumentException(
                    "Um horário vinculado a uma regra deve começar e terminar no mesmo dia.");

            var expectedDuration = TimeSpan.FromMinutes(SlotDurationMinutes);
            if (endsAt - startsAt != expectedDuration)
                throw new ArgumentException(
                    $"O horário deve ter exatamente {SlotDurationMinutes} minutos.");

            var startTime = TimeOnly.FromDateTime(startsAt);
            var endTime = TimeOnly.FromDateTime(endsAt);

            if (startTime < StartTime || endTime > EndTime)
                throw new ArgumentException(
                    "O horário deve estar dentro do intervalo definido pela regra.");

            var offsetFromRuleStart = startTime - StartTime;
            if (offsetFromRuleStart.Ticks % expectedDuration.Ticks != 0)
                throw new ArgumentException(
                    "O início do horário deve coincidir com um dos intervalos gerados pela regra.");
        }

        private static DayOfTheWeekEnum ToRuleDay(DayOfWeek dayOfWeek)
            => dayOfWeek switch
            {
                DayOfWeek.Monday => DayOfTheWeekEnum.Monday,
                DayOfWeek.Tuesday => DayOfTheWeekEnum.Tuesday,
                DayOfWeek.Wednesday => DayOfTheWeekEnum.Wednesday,
                DayOfWeek.Thursday => DayOfTheWeekEnum.Thursday,
                DayOfWeek.Friday => DayOfTheWeekEnum.Friday,
                DayOfWeek.Saturday => DayOfTheWeekEnum.Saturday,
                DayOfWeek.Sunday => DayOfTheWeekEnum.Sunday,
                _ => throw new ArgumentOutOfRangeException(nameof(dayOfWeek))
            };
    }
}
