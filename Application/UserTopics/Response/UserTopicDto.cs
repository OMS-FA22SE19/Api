using Application.Common.Mappings;
using Application.Types.Response;
using Application.Users.Response;
using AutoMapper;
using Core.Entities;

namespace Application.Topics.Response
{
    public sealed class UserTopicDto : IMapFrom<UserTopic>
    {
        public string UserId { get; set; }
        public UserDto User { get; set; }
        public void Mapping(Profile profile) => profile.CreateMap<UserTopic, UserTopicDto>().ReverseMap();
    }
}

