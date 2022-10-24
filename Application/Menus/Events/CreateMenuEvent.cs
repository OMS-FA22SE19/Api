using Core.Entities;
using Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Menus.Events
{
    public sealed class CreateMenuEvent : BaseEvent
    {
        public string Name { get; set; }
    }
    public sealed class CreateMenuEventHandler : INotificationHandler<CreateMenuEvent>
    {
        private readonly ILogger<CreateMenuEvent> _logger;
        public CreateMenuEventHandler(ILogger<CreateMenuEvent> logger)
        {
            _logger = logger;
        }

        public async Task Handle(CreateMenuEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Menu added: {0}", notification.Name);
        }
    }
}
