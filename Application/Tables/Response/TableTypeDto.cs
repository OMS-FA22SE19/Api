using Application.Categories.Response;
using Application.Common.Mappings;
using AutoMapper;
using Core.Entities;
using Core.Enums;

namespace Application.Tables.Response
{
    public class TableTypeDto : IMapFrom<Table>
    {
        public int NumOfSeats { get; set; }
        public TableType Type { get; set; }
        public int Total { get; set; }
        public IList<int> TableIds { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Table, TableTypeDto>().ReverseMap();
        }
    }
}
