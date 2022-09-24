using Core.Common.Interfaces;
using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Repositories
{
    public class MenuFoodRepository : GenericRepository<MenuFood>, IMenuFoodRepository
    {
        public MenuFoodRepository(IApplicationDbContext context) : base(context)
        {
            _dbSet = context.MenuFoods;
        }
    }
}
