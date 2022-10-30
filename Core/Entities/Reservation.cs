using Core.Enums;
using Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public sealed class Reservation : BaseAuditableEntity
    {
        [Required]
        public string UserId { get; set; }
        [Range(1, 1000)]
        public int NumOfPeople { get; set; }
        public int TableTypeId { get; set; }
        public int NumOfSeats { get; set; }
        public int Quantity { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public ReservationStatus Status { get; set; }
        public bool IsPriorFoodOrder { get; set; }

        public Order Order { get; set; }
        public ApplicationUser User { get; set; }
        public Billing Billing { get; set; }

        public IList<ReservationTable> ReservationTables { get; set; }
    }
}
