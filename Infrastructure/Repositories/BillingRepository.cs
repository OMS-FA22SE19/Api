using Core.Common.Interfaces;
using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Repositories
{
    public class BillingRepository : GenericRepository<Billing>, IBillingRepository
    {
        public BillingRepository(IApplicationDbContext context) : base(context)
        {
            _dbSet = context.Billings;
        }
    }
}
