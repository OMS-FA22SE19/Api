using Application.Common.Mappings;
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
        public int NumOfPeople { get; set; }
        public int TableTypeId { get; set; }
        public int NumOfSeats { get; set; }
        public int Quantity { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public ReservationStatus Status { get; set; }
        public bool IsPriorFoodOrder { get; set; }
        public IList<ReservationTableDto> ReservationTables { get; set; }
        public UserDto User { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Reservation, ReservationDto>().ReverseMap();
        }
    }
}
