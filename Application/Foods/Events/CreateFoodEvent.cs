using Core.Entities;
using Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.CourseTypes.Events
{
    public sealed class CreateFoodEvent : BaseEvent
    {
        public string Name { get; set; }
    }
    public sealed class CreateFoodEventHandler : INotificationHandler<CreateFoodEvent>
    {
        private readonly ILogger<CreateFoodEvent> _logger;
        public CreateFoodEventHandler(ILogger<CreateFoodEvent> logger)
        {
            _logger = logger;
        }

        public async Task Handle(CreateFoodEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Food added: {0}", notification.Name);
        }
    }
}
