using Application.Common.Mappings;
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
        public DateTime Date { get; set; }
        public ReservationStatus Status { get; set; }
        public bool IsPriorFoodOrder { get; set; }
        public Table Table { get; set; }
        public ApplicationUser User { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Reservation, ReservationDto>().ReverseMap();
        }
    }
}
