using Application.Common.Mappings;
using AutoMapper;
using Core.Entities;

namespace Application.Users
{
    public class UserDto : IMapFrom<ApplicationUser>
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsDeleted { get; set; }

        public void Mapping(Profile profile) => profile.CreateMap<ApplicationUser, UserDto>();
    }
}
