using Application.Menus.Response;
using Application.Models;
using Application.Topics.Response;
using AutoMapper;
using Core.Interfaces;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.UserTopics.Commands
{
    public sealed class DeleteUserFromTopicCommand : IRequest<Response<TopicDto>>
    {
        [JsonIgnore]
        public string UserId { get; set; }
        [JsonIgnore]
        public int TopicId { get; set; }
    }

    public sealed class DeleteUserFromTopicCommandHandler : IRequestHandler<DeleteUserFromTopicCommand, Response<TopicDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DeleteUserFromTopicCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<TopicDto>> Handle(DeleteUserFromTopicCommand request, CancellationToken cancellationToken)
        {

            var result = await _unitOfWork.UserTopicRepository.DeleteAsync(e => e.UserId == request.UserId && e.TopicId == request.TopicId);
            await _unitOfWork.CompleteAsync(cancellationToken);
            return new Response<TopicDto>()
            {
                Succeeded = true,
                StatusCode = System.Net.HttpStatusCode.NoContent
            };
        }
    }
}
