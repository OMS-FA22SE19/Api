using Application.Common.Mappings;
using AutoMapper;
using Core.Entities;
using Core.Enums;

namespace Application.Tables.Response
{
    public class TableDto : IMapFrom<Table>
    {
        public int Id { get; set; }
        public int NumOfSeats { get; set; }
        public TableStatus Status { get; set; }
        public TableType TableType { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Table, TableDto>()
                .ForMember(e => e.TableType, opt => opt.MapFrom(e => e.TableType)).ReverseMap();
        }
    }
}
