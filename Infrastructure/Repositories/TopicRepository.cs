using Core.Common.Interfaces;
using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Repositories
{
    public sealed class TopicRepository : AuditableEntityRepository<Topic>, ITopicRepository
    {
        public TopicRepository(IApplicationDbContext context) : base(context, context.Topics)
        {
        }
    }
}
