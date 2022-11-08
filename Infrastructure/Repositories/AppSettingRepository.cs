using Core.Common.Interfaces;
using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Repositories
{
    public class AdminSettingRepository : EntityRepository<AdminSetting>, IAdminSettingRepository
    {
        public AdminSettingRepository(IApplicationDbContext context) : base(context, context.AdminSettings)
        {
        }
    }
}
