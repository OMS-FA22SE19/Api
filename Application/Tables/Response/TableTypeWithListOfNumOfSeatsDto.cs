using Application.Common.Mappings;
using AutoMapper;
using Core.Entities;

namespace Application.Tables.Response
{
    public class TableTypeWithListOfNumOfSeatsDto : IMapFrom<Table>
    {
        public int TableTypeId { get; set; }
        public string TableTypeName { get; set; }
        public int numOfPeople { get; set; }
        public IList<TableByNumOfSeatDto> ListOfNumOfSeats { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Table, TableByTypeDto>()
                .ForMember(dto => dto.TableTypeName, opt => opt.MapFrom(e => e.TableType.Name))
                .ReverseMap();
        }
    }
}
