using Domain.Common;
using MediatR;
using Application.Common.Interfaces;
using Application.Notifications.Responses;
using Application.Common.Models;
using Application.Reservations.Response;

namespace Application.Notifications.Commands
{
    public sealed class PushNotificationCommands: IRequest<Response<NotificationDto>>
    {
        public string token { get; set; }
        public string title { get; set; }
        public string body { get; set; }
    }
    public sealed class PushNotificationCommandsHandler : IRequestHandler<PushNotificationCommands, Response<NotificationDto>>
    {
        private readonly IFirebaseMessagingService _messagingService;
        public PushNotificationCommandsHandler(IFirebaseMessagingService messagingService)
        {
            _messagingService = messagingService;
        }

        public async Task<Response<NotificationDto>> Handle(PushNotificationCommands command, CancellationToken cancellationToken)
        {
            var result = await _messagingService.SendNotification(command.token, command.title, command.body);

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
