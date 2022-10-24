using Core.Entities;
using Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Menus.Events
{
    public sealed class UpdateMenuEvent : BaseEvent
    {
        public int Id { get; set; }
    }
    public sealed class UpdateMenuEventHandler : INotificationHandler<UpdateMenuEvent>
    {
        private readonly ILogger<UpdateMenuEvent> _logger;
        public UpdateMenuEventHandler(ILogger<UpdateMenuEvent> logger)
        {
            _logger = logger;
        }

        public async Task Handle(UpdateMenuEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Menu id: {0} was changed", notification.Id);
        }
    }
}
