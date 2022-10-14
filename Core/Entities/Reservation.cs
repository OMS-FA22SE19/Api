using Core.Enums;
using Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public sealed class Reservation : BaseAuditableEntity
    {
        [Required]
        public string UserId { get; set; }
        //[Required]
        //public int TableId { get; set; }
        [Range(1, 1000)]
        public int NumOfPeople { get; set; }
        public int TableTypeId { get; set; }
        public int NumOfSeats { get; set; }
        public int Quantity { get; set; }
        public DateTime StartTime { get; set; } = DateTime.UtcNow.AddHours(7);
        public DateTime EndTime { get; set; } = DateTime.UtcNow.AddHours(7).AddHours(1).AddMinutes(30);
        public ReservationStatus Status { get; set; }
        public bool IsPriorFoodOrder { get; set; }

        //public Table Table { get; set; }
        public ApplicationUser User { get; set; }

        public IList<ReservationTable> ReservationTables { get; set; }
    }
}
