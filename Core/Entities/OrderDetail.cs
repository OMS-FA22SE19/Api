using Core.Enums;
using Domain.Common;

namespace Core.Entities
{
    public sealed class OrderDetail : BaseAuditableEntity
    {
        public string OrderId { get; set; }
        public int FoodId { get; set; }
        public double Price { get; set; }
        public OrderDetailStatus Status { get; set; }

        public Order Order { get; set; }
        public Food Food { get; set; }
    }
}
