using Core.Entities;
using Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.CourseTypes.Events
{
    public sealed class CourseTypeAddEvent : BaseEvent
    {
        public CourseType CourseType { get; set; }
    }
    public sealed class CourseTypeAddEventHandler : INotificationHandler<CourseTypeAddEvent>
    {
        private readonly ILogger<CourseTypeAddEvent> _logger;
        public CourseTypeAddEventHandler(ILogger<CourseTypeAddEvent> logger)
        {
            _logger = logger;
        }

        public async Task Handle(CourseTypeAddEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Course type added: {0}", notification.CourseType.Name);
        }
    }
}
