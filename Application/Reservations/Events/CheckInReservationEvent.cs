using Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using Core.Entities;
using Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using FirebaseAdmin.Messaging;

namespace Application.Orders.Events
{
    public sealed class CheckInReservationEvent : BaseEvent
    {
        public int ReservationId { get; set; }
        public List<int> tableIds { get; set; }
    }
    public sealed class CheckInReservationEventHandler : INotificationHandler<CheckInReservationEvent>
    {
        private readonly ILogger<CheckInReservationEvent> _logger;
        public CheckInReservationEventHandler(ILogger<CheckInReservationEvent> logger)
        {
            _logger = logger;
        }

        public async Task Handle(CheckInReservationEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Reservation id {0} check in with table {1}", notification.ReservationId, String.Join(", ", notification.tableIds));
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile("private_key.json")
            });
            var message = new Message()
            {
                Data = new Dictionary<string, string>() 
                {
                    { "myData", "1337" }
                },
                Topic = "all",
                Notification = new Notification()
                {
                    Title = "Your Table have been checked in",
                    Body = "Your Table have been checked in"
                }
            };

            string response = FirebaseMessaging.DefaultInstance.SendAsync(message).Result;
            Console.WriteLine("success: " + response);
        }
    }
}
