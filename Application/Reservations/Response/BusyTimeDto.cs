using Application.Categories.Response;
using Application.Common.Mappings;
using AutoMapper;
using Core.Entities;
using Core.Enums;

namespace Application.Reservations.Response
{
    public class BusyTimeDto : IMapFrom<Reservation>
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Reservation, BusyTimeDto>().ReverseMap();
        }
    }
}
