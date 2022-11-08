using Core.Common.Interfaces;
using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Repositories
{
    public class BillingRepository : EntityRepository<Billing>, IBillingRepository
    {
        public BillingRepository(IApplicationDbContext context) : base(context, context.Billings)
        {
        }
    }
}
