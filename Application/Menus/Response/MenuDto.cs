using Application.Common.Mappings;
using AutoMapper;
using Core.Entities;

namespace Application.Menus.Response
{
    public sealed class MenuDto : IMapFrom<Menu>, IEquatable<MenuDto>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsHidden { get; set; }
        public bool IsDeleted { get; set; }

        public bool Equals(MenuDto? other)
        {
            if (other is null)
            {
                return false;
            }
            return this.Id == other.Id
                && this.Name == other.Name
                && this.Description == other.Description
                && this.IsHidden == other.IsHidden
                && this.IsDeleted == other.IsDeleted;
        }
        public void Mapping(Profile profile) => profile.CreateMap<Menu, MenuDto>().ReverseMap();
    }

    public sealed class MenuDtoComparer : IEqualityComparer<MenuDto>
    {
        public bool Equals(MenuDto x, MenuDto y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.Id == y.Id
                && x.Name == y.Name
                && x.Description == y.Description
                && x.IsHidden == y.IsHidden
                && x.IsDeleted == y.IsDeleted;
        }

        public int GetHashCode(MenuDto menuDto)
        {
            if (Object.ReferenceEquals(menuDto, null)) return 0;

            int hashMenuDtoName = menuDto.Name == null ? 0 : menuDto.Name.GetHashCode();

            int hashMenuDtoDescription = menuDto.Description == null ? 0 : menuDto.Description.GetHashCode();

            int hashMenuDtoIsHidden = menuDto.IsHidden == null ? 0 : menuDto.IsHidden.GetHashCode();

            int hashMenuDtoIsDeleted = menuDto.IsDeleted == null ? 0 : menuDto.IsDeleted.GetHashCode();

            int hashMenuId = menuDto.Id.GetHashCode();

            return hashMenuDtoName ^ hashMenuDtoDescription ^ hashMenuDtoIsHidden ^ hashMenuDtoIsDeleted ^ hashMenuId;
        }
    }
}

