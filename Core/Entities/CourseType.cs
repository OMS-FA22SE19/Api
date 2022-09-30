using Domain.Common;

namespace Core.Entities
{
    public sealed class CourseType : BaseAuditableEntity
    {
        public string Name { get; set; }

        public IList<Food> Foods { get; set; }
    }
}
