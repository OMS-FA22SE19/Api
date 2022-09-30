using Core.Common.Interfaces;
using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Repositories
{
    public sealed class FoodTypeRepository : GenericRepository<FoodType>, IFoodTypeRepository
    {
        public FoodTypeRepository(IApplicationDbContext context) : base(context)
        {
            _dbSet = context.FoodTypes;
        }
    }
}
