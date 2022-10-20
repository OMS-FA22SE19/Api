using Application.Common.Mappings;
using AutoMapper;
using Core.Entities;

namespace Application.Menus.Response
{
    public sealed class MenuDto : IMapFrom<Menu>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsHidden { get; set; }
        public bool IsDeleted { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Menu, MenuDto>().ReverseMap();
        }
    }
}
