using Core.Enums;
using Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.OrderDetails.Events
{
    public sealed class UpdateOrderDetailEvent : BaseEvent
    {
        public int Id { get; set; }
        public OrderDetailStatus Status { get; set; }
    }
    public sealed class UpdateOrderDetailEventHandler : INotificationHandler<UpdateOrderDetailEvent>
    {
        private readonly ILogger<UpdateOrderDetailEvent> _logger;
        public UpdateOrderDetailEventHandler(ILogger<UpdateOrderDetailEvent> logger)
        {
            _logger = logger;
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
                default:
                    break;
            }
            _logger.LogInformation("Order Detail id: {0} was changed to status {1}", notification.Id, status);
        }
    }
}
