using Core.Common;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public sealed class Billing : Entity
    {
        [Key]
        public string Id { get; set; }
        public int ReservationId { get; set; }
        public double ReservationAmount { get; set; }
        public string ReservationEBillingId { get; set; }
        public string OrderId { get; set; }
        public double OrderAmount { get; set; }
        public string OrderEBillingId { get; set; }

        public Reservation Reservation { get; set; }
        public Order Order { get; set; }
    }
}
