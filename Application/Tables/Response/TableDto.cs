using Application.Common.Mappings;
using Application.TableTypes.Response;
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
        public bool IsDeleted { get; set; }
        public TableTypeDto TableType { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Table, TableDto>().ReverseMap();
        }
    }
}
