using Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

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

        }
    }
}
