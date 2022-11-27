using Domain.Common;
using MediatR;
using Application.Common.Interfaces;
using Application.Notifications.Responses;
using Application.Common.Models;
using Application.Reservations.Response;
using Core.Interfaces;
using Application.Common.Exceptions;
using Core.Entities;
using System.Linq.Expressions;

namespace Application.Notifications.Commands
{
    public sealed class CallStaffCommand : IRequest<Response<NotificationDto>>
    {
        public string body { get; set; }
    }
    public sealed class CallStaffCommandHandler : IRequestHandler<CallStaffCommand, Response<NotificationDto>>
    {
        private readonly IFirebaseMessagingService _messagingService;
        private readonly IUnitOfWork _unitOfWork;
        public CallStaffCommandHandler(IFirebaseMessagingService messagingService, IUnitOfWork unitOfWork)
        {
            _messagingService = messagingService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<NotificationDto>> Handle(CallStaffCommand command, CancellationToken cancellationToken)
        {
            List<Expression<Func<UserTopic, bool>>> filters = new();
            filters.Add(ut => ut.Topic.Name.Equals("Staff"));
            var users = await _unitOfWork.UserTopicRepository.GetAllAsync(filters, null, $"{nameof(UserTopic.Topic)}");
            if (users is null )
            {
                throw new NotFoundException("There is no User can be found in staff");
            }
            
            List<UserDeviceToken> userDevices = new List<UserDeviceToken>();
            foreach(var user in users) {
                var deviceToken = await _unitOfWork.UserDeviceTokenRepository.GetAsync(d=>d.userId.Equals(user.UserId));
                if (deviceToken is not null ) {
                    userDevices.Add(deviceToken);
                }
            }

            int numOfSuccess = 0;
            foreach(var device in userDevices)
            {
                var resultDevice = await _messagingService.SendNotification(device.deviceToken, "Customer call for staff", command.body);
                if (resultDevice is not null ) { numOfSuccess++; }
            }
            var result = await _messagingService.SendNotificationToTopic("Staff", "Customer call for staff", command.body);

            NotificationDto notificationDto = new NotificationDto();
            if (numOfSuccess != 0)
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
