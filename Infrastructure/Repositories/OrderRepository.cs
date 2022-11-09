using Core.Common.Interfaces;
using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Repositories
{
    public class OrderRepository : AuditableEntityRepository<Order>, IOrderRepository
    {
        public OrderRepository(IApplicationDbContext context) : base(context, context.Orders)
        {
        }
    }
}
