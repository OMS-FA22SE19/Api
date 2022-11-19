using Application.Common.Interfaces;
using Core.Enums;
using Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.OrderDetails.Events
{
    public sealed class UpdateOrderDetailEvent : BaseEvent
    {
        public int Id { get; set; }
        public string name { get; set; }
        public OrderDetailStatus Status { get; set; }
        public string token { get; set; }
    }
    public sealed class UpdateOrderDetailEventHandler : INotificationHandler<UpdateOrderDetailEvent>
    {
        private readonly ILogger<UpdateOrderDetailEvent> _logger;
        private readonly IFirebaseMessagingService _firebaseMessagingService;
        public UpdateOrderDetailEventHandler(ILogger<UpdateOrderDetailEvent> logger, IFirebaseMessagingService firebaseMessaging)
        {
            _logger = logger;
            _firebaseMessagingService = firebaseMessaging;
        }

        public async Task Handle(UpdateOrderDetailEvent notification, CancellationToken cancellationToken)
        {
            string status = "";
            switch (notification.Status)
            {
                case OrderDetailStatus.Cancelled:
                    status = "Cancelled";
                    break;
                case OrderDetailStatus.Processing:
                    status = "Processing";
                    break;
                case OrderDetailStatus.Served:
                    status = "Served";
                    break;
                case OrderDetailStatus.ReadyToServe:
                    status = "ReadyToServe";
                    break;
                default:
                    break;
            }
            _logger.LogInformation("Order Detail id: {0} was changed to status {1}", notification.Id, status);

            if (!string.IsNullOrWhiteSpace(notification.token))
            {
                var result = await _firebaseMessagingService.SendNotification(notification.token, "Order updated", $"Your {notification.name} was changed to {status}");
            }
        }
    }
}
