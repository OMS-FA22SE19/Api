using Domain.Common;

namespace Core.Entities
{
    public sealed class Topic : BaseAuditableEntity
    {
        public string Name { get; set; }

        public IList<UserTopic> UserTopics { get; set; }
    }
}
