using Core.Enums;
using Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public sealed class Reservation : BaseAuditableEntity
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public int TableId { get; set; }
        [Range(1, 1000)]
        public int NumOfPeople { get; set; } = 1;
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime EndTime { get; set; } = DateTime.UtcNow.AddHours(1).AddMinutes(30);
        public ReservationStatus Status { get; set; }
        public bool IsPriorFoodOrder { get; set; }

        public Table Table { get; set; }
        public ApplicationUser User { get; set; }
    }
}
