using Core.Common.Interfaces;
using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Repositories
{
    public class CourseTypeRepository : GenericRepository<CourseType>, ICourseTypeRepository
    {
        public CourseTypeRepository(IApplicationDbContext context) : base(context)
        {
            _dbSet = context.CourseTypes;
        }
    }
}
