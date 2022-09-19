using Core.Common.Interfaces;
using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Repositories
{
    public sealed class FoodCategoryRepository : GenericRepository<FoodCategory>, IFoodCategoryRepository
    {
        public FoodCategoryRepository(IApplicationDbContext context) : base(context)
        {
            _dbSet = context.FoodCategories;
        }
    }
}
