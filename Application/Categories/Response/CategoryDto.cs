using Application.Common.Mappings;
using AutoMapper;
using Core.Entities;

namespace Application.Categories.Response
{
    public sealed class CategoryDto : IMapFrom<Category>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsDeleted { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Category, CategoryDto>();
        }
    }
}
