using Core.Enums;
using Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public sealed class Reservation : BaseEntity
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public int TableId { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public ReservationStatus Status { get; set; }
        public bool IsPriorFoodOrder { get; set; }

        public Table Table { get; set; }
        public ApplicationUser User { get; set; }
    }
}
