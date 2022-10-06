using Core.Common;
using Core.Enums;
using Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public sealed class Payment : Entity
    {
        [Key]
        public new string Id { get; set; }
        [Required]
        public string OrderId { get; set; }
        public PaymentStatus Status { get; set; }
        public double Ammount { get; set; } = 0;
        public Order Order { get; set; }
    }
}
