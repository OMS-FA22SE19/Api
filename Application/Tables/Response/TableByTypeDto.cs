using Application.Common.Mappings;
using AutoMapper;
using Core.Entities;

namespace Application.Tables.Response
{
    public class TableByTypeDto : IMapFrom<Table>
    {
        public int NumOfSeats { get; set; }
        public int TableTypeId { get; set; }
        public string TableTypeName { get; set; }
        public int Quantity { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Table, TableByTypeDto>()
                .ForMember(dto => dto.TableTypeName, opt => opt.MapFrom(e => e.TableType.Name))
                .ReverseMap();
        }
    }
}
