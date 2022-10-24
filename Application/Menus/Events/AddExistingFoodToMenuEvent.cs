using Core.Entities;
using Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Menus.Events
{
    public sealed class AddExistingFoodToMenuEvent : BaseEvent
    {
        public string foodName { get; set; }
        public string menuName { get; set; }
    }
    public sealed class AddExistingFoodToMenuEventHandler : INotificationHandler<AddExistingFoodToMenuEvent>
    {
        private readonly ILogger<AddExistingFoodToMenuEvent> _logger;
        public AddExistingFoodToMenuEventHandler(ILogger<AddExistingFoodToMenuEvent> logger)
        {
            _logger = logger;
        }

        public async Task Handle(AddExistingFoodToMenuEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Food Name: {0} was added to Menu Name: {1}", notification.foodName, notification.menuName);
        }
    }
}
