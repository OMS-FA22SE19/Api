using Application.Common.Mappings;
using Application.OrderDetails.Response;
using Application.Users.Response;
using AutoMapper;
using Core.Entities;
using Core.Enums;

namespace Application.Demo.Response
{
    public class ReservationDemoDto
    {
        public List<int> ReservationCheckIn { get; set; }
        public List<int> ReservationAvailable { get; set; }
        public List<int> ReservationReserved { get; set; }
        public List<int> ReservationCancelled { get; set; }
    }
}
