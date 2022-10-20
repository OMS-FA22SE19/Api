using Application.Common.Mappings;
using AutoMapper;
using Core.Entities;

namespace Application.CourseTypes.Response
{
    public sealed class CourseTypeDto : IMapFrom<CourseType>, IEquatable<CourseTypeDto>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsDeleted { get; set; }

        public bool Equals(CourseTypeDto? other)
        {
            if (other is null)
            {
                return false;
            }

            return this.Id == other.Id && this.Name == other.Name;
        }
        public void Mapping(Profile profile) => profile.CreateMap<CourseType, CourseTypeDto>().ReverseMap();
    }

    public sealed class CourseTypeDtoComparer : IEqualityComparer<CourseTypeDto>
    {
        public bool Equals(CourseTypeDto x, CourseTypeDto y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.Id == y.Id && x.Name == y.Name;
        }

        public int GetHashCode(CourseTypeDto courseTypeDto)
        {
            if (Object.ReferenceEquals(courseTypeDto, null)) return 0;

            int hashCourseTypeDtoName = courseTypeDto.Name == null ? 0 : courseTypeDto.Name.GetHashCode();

            int hashProductId = courseTypeDto.Id.GetHashCode();

            return hashCourseTypeDtoName ^ hashProductId;
        }
    }
}
