using Application.Common.Mappings;
using AutoMapper;

namespace Application.Types.Response
{
    public sealed class TypeDto : IMapFrom<Core.Entities.Type>, IEquatable<TypeDto>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }

        public bool Equals(TypeDto? other)
        {
            if (other is null)
            {
                return false;
            }
            return this.Id == other.Id
                && this.Name == other.Name
                && this.IsDeleted == other.IsDeleted;
        }
        public void Mapping(Profile profile) => profile.CreateMap<Core.Entities.Type, TypeDto>().ReverseMap();
    }

    public sealed class TypeDtoComparer : IEqualityComparer<TypeDto>
    {
        public bool Equals(TypeDto x, TypeDto y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.Id == y.Id
                && x.Name == y.Name
                && x.IsDeleted == y.IsDeleted;
        }

        public int GetHashCode(TypeDto TypeDto)
        {
            if (Object.ReferenceEquals(TypeDto, null)) return 0;

            int hashTypeDtoName = TypeDto.Name == null ? 0 : TypeDto.Name.GetHashCode();

            int hashTypeDtoIsDeleted = TypeDto.IsDeleted == null ? 0 : TypeDto.IsDeleted.GetHashCode();

            int hashTypeId = TypeDto.Id.GetHashCode();

            return hashTypeDtoName ^ hashTypeDtoIsDeleted ^ hashTypeId;
        }
    }
}

