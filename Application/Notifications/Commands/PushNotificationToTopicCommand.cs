using Domain.Common;
using MediatR;
using Application.Common.Interfaces;
using Application.Notifications.Responses;
using Application.Models;
using Application.Reservations.Response;

namespace Application.Notifications.Commands
{
    public sealed class PushNotificationToTopicCommand : IRequest<Response<NotificationDto>>
    {
        public string topic { get; set; }
        public string title { get; set; }
        public string body { get; set; }
    }
    public sealed class PushNotificationToTopicCommandHandler : IRequestHandler<PushNotificationToTopicCommand, Response<NotificationDto>>
    {
        private readonly IFirebaseMessagingService _messagingService;
        public PushNotificationToTopicCommandHandler(IFirebaseMessagingService messagingService)
        {
            _messagingService = messagingService;
        }

        public async Task<Response<NotificationDto>> Handle(PushNotificationToTopicCommand command, CancellationToken cancellationToken)
        {
            var result = await _messagingService.SendNotificationToTopic(command.topic, command.title, command.body);

            NotificationDto notificationDto = new NotificationDto();
            if (result is not null)
            {
                notificationDto.IsSuccess = true;
                notificationDto.Message = "Send Successful";
            }
            else
            {
                notificationDto.IsSuccess = false;
                notificationDto.Message = "Not Success";
            }
            return new Response<NotificationDto>(notificationDto);
        }
    }
}
