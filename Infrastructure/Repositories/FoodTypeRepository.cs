using Core.Common.Interfaces;
using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Repositories
{
    public sealed class FoodTypeRepository : EntityRepository<FoodType>, IFoodTypeRepository
    {
        public FoodTypeRepository(IApplicationDbContext context) : base(context, context.FoodTypes)
        {
        }
    }
}
