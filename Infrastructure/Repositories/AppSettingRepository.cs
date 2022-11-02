using Core.Common.Interfaces;
using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Repositories
{
    public class AdminSettingRepository : GenericRepository<AdminSetting>, IAdminSettingRepository
    {
        public AdminSettingRepository(IApplicationDbContext context) : base(context)
        {
            _dbSet = context.AdminSettings;
        }
    }
}
