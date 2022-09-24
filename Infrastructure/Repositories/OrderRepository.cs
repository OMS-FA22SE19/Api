using Core.Common.Interfaces;
using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Repositories
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(IApplicationDbContext context) : base(context)
        {
            _dbSet = context.Orders;
        }
    }
}
