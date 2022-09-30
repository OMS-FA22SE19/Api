using Application.Common.Mappings;
using Core.Entities;

namespace Application.Reservations.Response
{
    public class BusyTimeDto : IMapFrom<Reservation>
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
