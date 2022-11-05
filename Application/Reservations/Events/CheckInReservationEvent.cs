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
using Application.Common.Interfaces;

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
        private readonly IFirebaseMessagingService _messagingService;
        public CheckInReservationEventHandler(ILogger<CheckInReservationEvent> logger, IFirebaseMessagingService messagingService)
        {
            _logger = logger;
            _messagingService = messagingService;
        }

        public async Task Handle(CheckInReservationEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Reservation id {0} check in with table {1}", notification.ReservationId, String.Join(", ", notification.tableIds));
            //var app = FirebaseApp.Create(new AppOptions()
            //{
            //    Credential = GoogleCredential.FromFile("private_key.json").CreateScoped("https://www.googleapis.com/auth/firebase.messaging")
            //});
            //var messaging = FirebaseMessaging.GetMessaging(app);
            //var message = new Message()
            //{
            //    Data = new Dictionary<string, string>() 
            //    {
            //        { "ReservationId", notification.ReservationId.ToString() }
            //    },
            //    Token = "cotIxn8ryWTr9UOUf57GKA:APA91bGIRh2GLZEphDnfSHaFK7IMY0Sh9QBGJ9ngiaHLCxLfjLiXwc7WyqvgRvog5qrJ-w59h_k4sVOTnkhAb88e7n4GFcwvQ3Yhixblrnt32HzrPDGQQyL3DBuoLqf_j0EV5CBwv2wk",
            //    Notification = new Notification()
            //    {
            //        Title = "Your Reservation have been checked in",
            //        Body = "Your Reservation have been checked in"
            //    }
            //};

            //var token = "cotIxn8ryWTr9UOUf57GKA:APA91bGIRh2GLZEphDnfSHaFK7IMY0Sh9QBGJ9ngiaHLCxLfjLiXwc7WyqvgRvog5qrJ-w59h_k4sVOTnkhAb88e7n4GFcwvQ3Yhixblrnt32HzrPDGQQyL3DBuoLqf_j0EV5CBwv2wk";
            //var title = "check in";
            //var body = "Your Reservation have been checked in";
            //_messagingService.SendNotification(token, title, body);

            //string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            //Console.WriteLine("success: " + response);
        }
    }
}
