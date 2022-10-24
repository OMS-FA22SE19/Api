using Core.Entities;
using Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Foods.Events
{
    public sealed class UpdateFoodEvent : BaseEvent
    {
        public int Id { get; set; }
    }
    public sealed class UpdateFoodEventHandler : INotificationHandler<UpdateFoodEvent>
    {
        private readonly ILogger<UpdateFoodEvent> _logger;
        public UpdateFoodEventHandler(ILogger<UpdateFoodEvent> logger)
        {
            _logger = logger;
        }

        public async Task Handle(UpdateFoodEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Food id: {0} was changed", notification.Id);
        }
    }
}
