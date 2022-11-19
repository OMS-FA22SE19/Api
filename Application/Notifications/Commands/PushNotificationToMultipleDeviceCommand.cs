using Domain.Common;
using MediatR;
using Application.Common.Interfaces;
using Application.Notifications.Responses;
using Application.Common.Models;
using Application.Reservations.Response;

namespace Application.Notifications.Commands
{
    public sealed class PushNotificationToMultipleDeviceCommand : IRequest<Response<NotificationDto>>
    {
        public List<string> token { get; set; }
        public string title { get; set; }
        public string body { get; set; }
    }
    public sealed class PushNotificationToMultipleDeviceCommandHandler : IRequestHandler<PushNotificationToMultipleDeviceCommand, Response<NotificationDto>>
    {
        private readonly IFirebaseMessagingService _messagingService;
        public PushNotificationToMultipleDeviceCommandHandler(IFirebaseMessagingService messagingService)
        {
            _messagingService = messagingService;
        }

        public async Task<Response<NotificationDto>> Handle(PushNotificationToMultipleDeviceCommand command, CancellationToken cancellationToken)
        {
            var result = await _messagingService.SendNotificationToMultipleDevice(command.token, command.title, command.body);

            NotificationDto notificationDto = new NotificationDto();
            if (result != 0)
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
