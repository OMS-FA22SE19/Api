using Application.Common.Mappings;
using Application.CourseTypes.Response;
using Application.Types.Response;
using AutoMapper;
using Core.Entities;

namespace Application.Foods.Response
{
    public sealed class FoodDto : IMapFrom<Food>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Ingredient { get; set; }
        public bool Available { get; set; } = true;
        public string PictureUrl { get; set; }
        public bool IsDeleted { get; set; }
        public CourseTypeDto CourseType { get; set; }
        public IList<TypeDto> Types { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Food, FoodDto>().ReverseMap();
        }
    }
}
