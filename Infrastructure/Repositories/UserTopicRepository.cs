using Core.Common.Interfaces;
using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Repositories
{
    public sealed class UserTopicRepository : EntityRepository<UserTopic>, IUserTopicRepository
    {
        public UserTopicRepository(IApplicationDbContext context) : base(context, context.UserTopics)
        {
        }
    }
}
