using Core.Entities;
using Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Foods.Events
{
    public sealed class DeleteFoodEvent : BaseEvent
    {
        public int id { get; set; }
    }
    public sealed class DeleteFoodEventHandler : INotificationHandler<DeleteFoodEvent>
    {
        private readonly ILogger<DeleteFoodEvent> _logger;
        public DeleteFoodEventHandler(ILogger<DeleteFoodEvent> logger)
        {
            _logger = logger;
        }

        public async Task Handle(DeleteFoodEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Food id: {0} was deleted", notification.id);
        }
    }
}
