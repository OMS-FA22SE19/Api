using Core.Entities;
using Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Orders.Events
{
    public sealed class CreateOrderEvent : BaseEvent
    {
        public string id { get; set; }
    }
    public sealed class CreateOrderEventHandler : INotificationHandler<CreateOrderEvent>
    {
        private readonly ILogger<CreateOrderEvent> _logger;
        public CreateOrderEventHandler(ILogger<CreateOrderEvent> logger)
        {
            _logger = logger;
        }

        public async Task Handle(CreateOrderEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Order added: {0}", notification.id);
        }
    }
}
