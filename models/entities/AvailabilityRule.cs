
using Reservae.Models.Common;

namespace Reservae.Models
{
    public class AvailabilityRule : AuditableEntity
    {
        public int Id { get; set; }
        public int SpaceId { get; set; }
        public Space Space { get; set; } = null!;
        public DayOfTheWeekEnum DayOfTheWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidUntil { get; set; }
        public decimal CustomPricePerSpot { get; set; }
        public bool IsActive { get; set; }
        public int Capacity { get; set; }
        public ICollection<BookableSlot> BookableSlots { get; set; } = new List<BookableSlot>();
    }
}
