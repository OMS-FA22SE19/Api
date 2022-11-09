using Core.Common.Interfaces;
using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Repositories
{
    public class MenuRepository : AuditableEntityRepository<Menu>, IMenuRepository
    {
        public MenuRepository(IApplicationDbContext context) : base(context, context.Menus)
        {
        }
    }
}
