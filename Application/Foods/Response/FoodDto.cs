using Application.Categories.Response;
using Application.Common.Mappings;
using AutoMapper;
using Core.Entities;

namespace Application.Foods.Response
{
    public class FoodDto : IMapFrom<Food>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Ingredient { get; set; }
        public bool Available { get; set; } = true;
        public string PictureUrl { get; set; }
        public IList<CategoryDto> Categories { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Food, FoodDto>().ReverseMap();
        }
    }
}
