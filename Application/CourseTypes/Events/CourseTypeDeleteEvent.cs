using Core.Entities;
using Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.CourseTypes.Events
{
    public sealed class CourseTypeDeleteEvent : BaseEvent
    {
        public int id { get; set; }
    }
    public sealed class CourseTypeDeleteEventHandler : INotificationHandler<CourseTypeDeleteEvent>
    {
        private readonly ILogger<CourseTypeDeleteEvent> _logger;
        public CourseTypeDeleteEventHandler(ILogger<CourseTypeDeleteEvent> logger)
        {
            _logger = logger;
        }

        public async Task Handle(CourseTypeDeleteEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Course type {0} was deleted", notification.id);
        }
    }
}
