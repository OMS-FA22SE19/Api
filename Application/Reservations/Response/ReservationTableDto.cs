using Application.Common.Mappings;
using Application.Tables.Response;
using AutoMapper;
using Core.Entities;

namespace Application.Reservations.Response
{
    public class ReservationTableDto : IMapFrom<ReservationTable>
    {
        //public int ReservationId { get; set; }
        public int TableId { get; set; }
        public TableDto Table { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ReservationTable, ReservationTableDto>().ReverseMap();
        }
    }
}
