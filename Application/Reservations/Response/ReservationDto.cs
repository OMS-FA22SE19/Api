using Application.Common.Mappings;
using Application.Tables.Response;
using Application.Users;
using AutoMapper;
using Core.Entities;
using Core.Enums;

namespace Application.Reservations.Response
{
    public class ReservationDto : IMapFrom<Reservation>
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int TableId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public ReservationStatus Status { get; set; }
        public bool IsPriorFoodOrder { get; set; }
        public TableDto Table { get; set; }
        public UserDto User { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Reservation, ReservationDto>().ReverseMap();
        }
    }
}
