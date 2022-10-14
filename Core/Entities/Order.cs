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
        public string UserId { get; set; }
        [Required]
        public int ReservationId { get; set; }
        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow.AddHours(7);
        public OrderStatus Status { get; set; }
        [Range(0, double.PositiveInfinity)]
        public double PrePaid { get; set; } = 0;

        public ApplicationUser User { get; set; }
        public Reservation Reservation { get; set; }
        public IList<OrderDetail> OrderDetails { get; set; }
        public IList<Payment> Payments { get; set; }
    }
}
