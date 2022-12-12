using Core.Enums;
using Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public sealed class Order : BaseAuditableEntity
    {
        [Key]
        public new string Id { get; set; }
        [Required]
        public int ReservationId { get; set; }
        [Required]
        public DateTime Date { get; set; }
        public OrderStatus Status { get; set; }
        [Range(0, double.PositiveInfinity)]
        public double PrePaid { get; set; } = 0;
        public Reservation Reservation { get; set; }
        public Billing Billing { get; set; }

        public IList<OrderDetail> OrderDetails { get; set; }
    }
}
