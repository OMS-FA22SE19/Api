using Core.Common.Interfaces;
using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Repositories
{
    public class CourseTypeRepository : AuditableEntityRepository<CourseType>, ICourseTypeRepository
    {
        public CourseTypeRepository(IApplicationDbContext context) : base(context, context.CourseTypes)
        {
        }
    }
}
