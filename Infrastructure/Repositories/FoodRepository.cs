using Core.Common.Interfaces;
using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Repositories
{
    public sealed class FoodRepository : AuditableEntityRepository<Food>, IFoodRepository
    {
        public FoodRepository(IApplicationDbContext context) : base(context, context.Foods)
        {
        }
    }
}
