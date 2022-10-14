using Application.Common.Mappings;
using AutoMapper;
using Core.Entities;

namespace Application.TableTypes.Response
{
    public sealed class TableTypeDto : IMapFrom<TableType>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double ChargePerSeat { get; set; }
        public bool CanBeCombined { get; set; }

        public void Mapping(Profile profile) => profile.CreateMap<TableType, TableTypeDto>();
    }
}
