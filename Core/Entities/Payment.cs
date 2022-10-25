using Core.Common;
using Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public sealed class Payment : Entity
    {
        [Key]
        public new string Id { get; set; }
        [Required]
        public int ReservationId { get; set; }
        public PaymentStatus Status { get; set; }
        public double Amount { get; set; }
        public Reservation reservation { get; set; }
    }
}
