using Application.Common.Mappings;
using Application.TableTypes.Response;
using AutoMapper;
using Core.Entities;
using Core.Enums;

namespace Application.Tables.Response
{
    public class TableStatusDto : IMapFrom<Table>
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? FullName { get; set; }
        public string? OrderId { get; set; }
        public TableStatus Status { get; set; }
        public TableTypeDto TableType { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Table, TableStatusDto>().ReverseMap();
        }
    }
}