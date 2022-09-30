using Application.Common.Mappings;
using AutoMapper;

namespace Application.Types.Response
{
    public sealed class TypeDto : IMapFrom<Core.Entities.Type>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsDeleted { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Core.Entities.Type, TypeDto>();
        }
    }
}
