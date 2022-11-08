using Core.Common.Interfaces;
using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Repositories
{
    public class OrderDetailRepository : AuditableEntityRepository<OrderDetail>, IOrderDetailRepository
    {
        public OrderDetailRepository(IApplicationDbContext context) : base(context, context.OrderDetails)
        {
        }
    }
}
