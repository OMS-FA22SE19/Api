using Core.Common;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public sealed class ReservationTable : Entity
    {
        [Required]
        public int ReservationId { get; set; }
        [Required]
        public int TableId { get; set; }

        public Reservation Reservation { get; set; }
        public Table Table { get; set; }
    }
}
