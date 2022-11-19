using Application.Common.Interfaces;
using Application.Menus.Response;
using Application.Common.Models;
using Application.Topics.Response;
using AutoMapper;
using Core.Interfaces;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.UserTopics.Commands
{
    public sealed class DeleteUserFromTopicCommand : IRequest<Response<TopicDto>>
    {
        public string UserId { get; set; }
        public int TopicId { get; set; }
    }

    public sealed class DeleteUserFromTopicCommandHandler : IRequestHandler<DeleteUserFromTopicCommand, Response<TopicDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFirebaseMessagingService _firebaseMessagingService;

        public DeleteUserFromTopicCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IFirebaseMessagingService firebaseMessagingService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _firebaseMessagingService = firebaseMessagingService;
        }

        public async Task<Response<TopicDto>> Handle(DeleteUserFromTopicCommand request, CancellationToken cancellationToken)
        {

            var result = await _unitOfWork.UserTopicRepository.DeleteAsync(e => e.UserId == request.UserId && e.TopicId == request.TopicId);
            await _unitOfWork.CompleteAsync(cancellationToken);

            var topic = await _unitOfWork.TopicRepository.GetAsync(t => t.Id == request.TopicId);
            var device = await _unitOfWork.UserDeviceTokenRepository.GetAsync(u => u.userId.Equals(request.UserId));
            List<string> tokens = new List<string>();
            tokens.Add(device.deviceToken);
            await _firebaseMessagingService.UnsubcribeFromTopic(tokens, topic.Name);

            return new Response<TopicDto>()
            {
                Succeeded = true,
                StatusCode = System.Net.HttpStatusCode.NoContent
            };
        }
    }
}
