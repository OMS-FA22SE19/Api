using Domain.Common;
using MediatR;
using Application.Common.Interfaces;
using Application.Notifications.Responses;
using Application.Common.Models;
using Application.Reservations.Response;
using Core.Interfaces;

namespace Application.Notifications.Commands
{
    public sealed class CallStaffCommand : IRequest<Response<NotificationDto>>
    {
        public string body { get; set; }
    }
    public sealed class CallStaffCommandHandler : IRequestHandler<CallStaffCommand, Response<NotificationDto>>
    {
        private readonly IFirebaseMessagingService _messagingService;
        public CallStaffCommandHandler(IFirebaseMessagingService messagingService)
        {
            _messagingService = messagingService;
        }

        public async Task<Response<NotificationDto>> Handle(CallStaffCommand command, CancellationToken cancellationToken)
        {
            var result = await _messagingService.SendNotificationToTopic("Staff", "Customer call for staff", command.body);

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
