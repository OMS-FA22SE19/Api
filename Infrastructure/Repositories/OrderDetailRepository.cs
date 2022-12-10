using Core.Common.Interfaces;
using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Repositories
{
    public class OrderDetailRepository : EntityRepository<OrderDetail>, IOrderDetailRepository
    {
        public OrderDetailRepository(IApplicationDbContext context) : base(context, context.OrderDetails)
        {
        }
    }
}
