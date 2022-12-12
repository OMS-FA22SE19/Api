using Core.Enums;
using Domain.Common;

namespace Core.Entities
{
    public sealed class OrderDetail : BaseEntity, IEquatable<OrderDetail>
    {
        public string OrderId { get; set; }
        public int FoodId { get; set; }
        public double Price { get; set; }
        public OrderDetailStatus Status { get; set; }
        public string Note { get; set; }
        public DateTime Created { get; set; }

        public Order Order { get; set; }
        public Food Food { get; set; }

        public bool Equals(OrderDetail? other)
        {
            if (other is null)
            {
                return false;
            }
            return this.OrderId == other.OrderId
                && this.FoodId == other.FoodId
                && this.Price == other.Price
                && this.Status == other.Status
                && this.Note == other.Note;
        }
    }

    public sealed class OrderDetailComparer : IEqualityComparer<OrderDetail>
    {
        public bool Equals(OrderDetail x, OrderDetail y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.OrderId == y.OrderId
                && x.FoodId == y.FoodId
                && x.Price == y.Price
                && x.Status == y.Status
                && x.Note == y.Note;
        }

        public int GetHashCode(OrderDetail OrderDetail)
        {
            if (Object.ReferenceEquals(OrderDetail, null)) return 0;

            int hashOrderDetailOrderId = OrderDetail.OrderId.GetHashCode();

            int hashOrderDetailFoodId = OrderDetail.FoodId.GetHashCode();

            int hashOrderDetailPrice = OrderDetail.Price == null ? 0 : OrderDetail.Price.GetHashCode();

            int hashOrderDetailStatus = OrderDetail.Status == null ? 0 : OrderDetail.Status.GetHashCode();

            int hashOrderDetailNote = OrderDetail.Note == null ? 0 : OrderDetail.Note.GetHashCode();


            return hashOrderDetailOrderId ^ hashOrderDetailFoodId ^ hashOrderDetailPrice ^ hashOrderDetailStatus ^ hashOrderDetailNote;
        }
    }
}
