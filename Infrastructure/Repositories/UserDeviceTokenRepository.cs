using Core.Common.Interfaces;
using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Repositories
{
    public sealed class UserDeviceTokenRepository : GenericRepository<UserDeviceToken>, IUserDeviceTokenRepository
    {
        public UserDeviceTokenRepository(IApplicationDbContext context) : base(context)
        {
            _dbSet = context.UserDeviceTokens;
        }
    }
}
