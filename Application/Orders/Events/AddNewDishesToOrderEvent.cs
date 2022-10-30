using Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using Core.Entities;
using Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Orders.Events
{
    public sealed class AddNewDishesToOrderEvent : BaseEvent
    {
        public string id { get; set; }
    }
    public sealed class AddNewDishesToOrderEventHandler : INotificationHandler<AddNewDishesToOrderEvent>
    {
        private readonly ILogger<AddNewDishesToOrderEvent> _logger;
        public AddNewDishesToOrderEventHandler(ILogger<AddNewDishesToOrderEvent> logger)
        {
            _logger = logger;
        }

        public async Task Handle(AddNewDishesToOrderEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Order id {0} added new dishes", notification.id);
        }
    }
}
