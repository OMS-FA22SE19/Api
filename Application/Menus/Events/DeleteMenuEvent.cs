using Core.Entities;
using Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Menus.Events
{
    public sealed class DeleteMenuEvent : BaseEvent
    {
        public int id { get; set; }
    }
    public sealed class DeleteMenuEventHandler : INotificationHandler<DeleteMenuEvent>
    {
        private readonly ILogger<DeleteMenuEvent> _logger;
        public DeleteMenuEventHandler(ILogger<DeleteMenuEvent> logger)
        {
            _logger = logger;
        }

        public async Task Handle(DeleteMenuEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Menu id: {0} was deleted", notification.id);
        }
    }
}
