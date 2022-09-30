using Application.Common.Mappings;
using AutoMapper;
using Core.Entities;

namespace Application.CourseTypes.Response
{
    public sealed class CourseTypeDto : IMapFrom<CourseType>
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public void Mapping(Profile profile) => profile.CreateMap<CourseType, CourseTypeDto>().ReverseMap();
    }
}
