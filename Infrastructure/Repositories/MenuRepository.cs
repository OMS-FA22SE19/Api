using Core.Common.Interfaces;
using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Repositories
{
    public class MenuRepository : GenericRepository<Menu>, IMenuRepository
    {
        public MenuRepository(IApplicationDbContext context) : base(context)
        {
            _dbSet = context.Menus;
        }
    }
}
