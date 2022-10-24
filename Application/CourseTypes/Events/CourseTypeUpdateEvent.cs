using Core.Entities;
using Domain.Common;
using FirebaseAdmin.Messaging;
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
            /*// This registration token comes from the client FCM SDKs.
            var registrationToken = "77287722969";

            // See documentation on defining a message payload.
            var message = new Message()
            {
                Notification = new Notification()
                {
                    Title = "Course Type " + notification.CourseType.Id + " got updated",
                    Body = "Course type Id: " + notification.CourseType.Id + " change name to:" + notification.CourseType.Name
                },
                Token = registrationToken,
            };

            // Send a message to the device corresponding to the provided
            // registration token.
            string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            // Response is a message ID string.
            Console.WriteLine("Successfully sent message: " + response);*/
            _logger.LogInformation("Course type {0} change name to: {1}", notification.CourseType.Id, notification.CourseType.Name);
        }
    }
}
