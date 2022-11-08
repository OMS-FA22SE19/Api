using Core.Common.Interfaces;
using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Repositories
{
    public sealed class UserDeviceTokenRepository : EntityRepository<UserDeviceToken>, IUserDeviceTokenRepository
    {
        public UserDeviceTokenRepository(IApplicationDbContext context) : base(context, context.UserDeviceTokens)
        {
        }
    }
}
