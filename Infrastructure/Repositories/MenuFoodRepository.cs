using Core.Common.Interfaces;
using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Repositories
{
    public class MenuFoodRepository : EntityRepository<MenuFood>, IMenuFoodRepository
    {
        public MenuFoodRepository(IApplicationDbContext context) : base(context, context.MenuFoods)
        {
        }
    }
}
