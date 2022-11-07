using Core.Common.Interfaces;
using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Repositories
{
    public sealed class UserTopicRepository : GenericRepository<UserTopic>, IUserTopicRepository
    {
        public UserTopicRepository(IApplicationDbContext context) : base(context)
        {
            _dbSet = context.UserTopics;
        }
    }
}
