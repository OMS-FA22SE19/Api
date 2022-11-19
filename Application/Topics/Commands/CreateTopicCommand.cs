using Application.Common.Mappings;
using Application.Common.Models;
using Application.Topics.Response;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Topics.Commands
{
    public class CreateTopicCommand : IMapFrom<Topic>, IRequest<Response<TopicDto>>
    {
        [Required]
        public string Name { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<CreateTopicCommand, Topic>();
        }
    }

    public class CreateTopicCommandHandler : IRequestHandler<CreateTopicCommand, Response<TopicDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateTopicCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<TopicDto>> Handle(CreateTopicCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Topic>(request);
            var result = await _unitOfWork.TopicRepository.InsertAsync(entity);
            await _unitOfWork.CompleteAsync(cancellationToken);
            if (result is null)
            {
                return new Response<TopicDto>("error");
            }
            var mappedResult = _mapper.Map<TopicDto>(result);
            return new Response<TopicDto>(mappedResult)
            {
                StatusCode = System.Net.HttpStatusCode.Created
            };
        }
    }
}
