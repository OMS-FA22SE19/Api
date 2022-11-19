using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Menus.Response;
using Application.Common.Models;
using Application.Topics.Response;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Application.UserTopics.Commands
{
    public sealed class AddUserToTopicCommand : IRequest<Response<TopicDto>>
    {
        [JsonIgnore]
        public string UserId { get; set; }
        [JsonIgnore]
        public int TopicId { get; set; }
    }

    public sealed class AddUserToTopicCommandHandler : IRequestHandler<AddUserToTopicCommand, Response<TopicDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFirebaseMessagingService _firebaseMessagingService;

        public AddUserToTopicCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IFirebaseMessagingService firebaseMessagingService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _firebaseMessagingService = firebaseMessagingService;
        }

        public async Task<Response<TopicDto>> Handle(AddUserToTopicCommand request, CancellationToken cancellationToken)
        {
            var userInDatabase = await _unitOfWork.UserRepository.GetAsync(e => e.Id == request.UserId && !e.IsDeleted);
            if (userInDatabase is null)
            {
                throw new NotFoundException(nameof(ApplicationUser), request.UserId);
            }

            var userMapped = _mapper.Map<ApplicationUser>(userInDatabase);

            var topicInDatabase = await _unitOfWork.TopicRepository.GetAsync(e => e.Id == request.TopicId && !e.IsDeleted);
            if (topicInDatabase is null)
            {
                throw new NotFoundException(nameof(Topic), request.TopicId);
            }

            var checkInDatabase = await _unitOfWork.UserTopicRepository.GetAsync(e => e.UserId == request.UserId && e.TopicId == request.TopicId) != null;
            if (checkInDatabase)
            {
                return new Response<TopicDto>($"This user {request.UserId} has already included this topic {request.TopicId}");
            }

            await _unitOfWork.UserTopicRepository.InsertAsync(new UserTopic
            {
                UserId = request.UserId,
                TopicId = request.TopicId
            });

            await _unitOfWork.TopicRepository.UpdateAsync(topicInDatabase);

            await _unitOfWork.CompleteAsync(cancellationToken);

            var device = await _unitOfWork.UserDeviceTokenRepository.GetAsync(u => u.userId.Equals(request.UserId));
            List<string> tokens = new List<string>();
            tokens.Add(device.deviceToken);
            await _firebaseMessagingService.SubcribeFromTopic(tokens, topicInDatabase.Name);

            return new Response<TopicDto>()
            {
                Succeeded = true,
                StatusCode = System.Net.HttpStatusCode.Created
            };
        }
    }
}
