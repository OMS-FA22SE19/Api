using Application.Common.Mappings;
using Application.Types.Response;
using Application.Users.Response;
using AutoMapper;
using Core.Entities;

namespace Application.Topics.Response
{
    public sealed class TopicDto : IMapFrom<Topic>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsDeleted { get; set; }
        public IList<UserTopicDto> UserTopics { get; set; }
        public void Mapping(Profile profile) => profile.CreateMap<Topic, TopicDto>().ReverseMap();
    }
}

