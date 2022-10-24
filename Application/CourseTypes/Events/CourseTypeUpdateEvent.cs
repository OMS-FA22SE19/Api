using Core.Entities;
using Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.CourseTypes.Events
{
    public sealed class CourseTypeUpdateEvent : BaseEvent
    {
        public CourseType CourseType { get; set; }
    }
    public sealed class CourseTypeUpdateEventHandler : INotificationHandler<CourseTypeUpdateEvent>
    {
        private readonly ILogger<CourseTypeUpdateEvent> _logger;
        public CourseTypeUpdateEventHandler(ILogger<CourseTypeUpdateEvent> logger)
        {
            _logger = logger;
        }

        public async Task Handle(CourseTypeUpdateEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Course type {0} change name to: {1}", notification.CourseType.Id, notification.CourseType.Name);
        }
    }
}
