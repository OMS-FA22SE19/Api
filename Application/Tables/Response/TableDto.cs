using Application.Categories.Response;
using Application.Common.Mappings;
using AutoMapper;
using Core.Entities;
using Core.Enums;

namespace Application.Tables.Response
{
    public class TableDto : IMapFrom<Food>
    {
        public int Id { get; set; }
        public string NumOfSeats { get; set; }
        public TableStatus Status { get; set; }
        public TableType Type { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Table, TableDto>().ReverseMap();
        }
    }
}
