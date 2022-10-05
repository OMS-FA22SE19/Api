using Domain.Common;

namespace Core.Entities
{
    public sealed class CourseType : BaseAuditableEntity, IEquatable<CourseType>
    {
        public string Name { get; set; }

        public IList<Food> Foods { get; set; }

        public bool Equals(CourseType? other)
        {
            if (other is null)
            {
                return false;
            }
            return this.Id == other.Id && this.Name == other.Name;
        }
    }

    public sealed class CourseTypeComparer : IEqualityComparer<CourseType>
    {
        public bool Equals(CourseType x, CourseType y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.Id == y.Id && x.Name == y.Name;
        }

        public int GetHashCode(CourseType courseType)
        {
            if (Object.ReferenceEquals(courseType, null)) return 0;

            int hashCourseTypeName = courseType.Name == null ? 0 : courseType.Name.GetHashCode();

            int hashProductId = courseType.Id.GetHashCode();

            return hashCourseTypeName ^ hashProductId;
        }
    }
}
