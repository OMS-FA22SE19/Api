using Application.Common.Mappings;
using AutoMapper;
using Core.Entities;

namespace Application.TableTypes.Response
{
    public sealed class TableTypeDto : IMapFrom<TableType>, IEquatable<TableTypeDto>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double ChargePerSeat { get; set; }
        public bool CanBeCombined { get; set; }

        public bool Equals(TableTypeDto? other)
        {
            if (other is null)
            {
                return false;
            }
            return this.Id == other.Id
                && this.Name == other.Name
                && this.ChargePerSeat == other.ChargePerSeat
                && this.CanBeCombined == other.CanBeCombined;
        }
        public void Mapping(Profile profile) => profile.CreateMap<TableType, TableTypeDto>().ReverseMap();
    }

    public sealed class TableTypeDtoComparer : IEqualityComparer<TableTypeDto>
    {
        public bool Equals(TableTypeDto x, TableTypeDto y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.Id == y.Id
                && x.Name == y.Name
                && x.ChargePerSeat == y.ChargePerSeat
                && x.CanBeCombined == y.CanBeCombined;
        }

        public int GetHashCode(TableTypeDto TableTypeDto)
        {
            if (Object.ReferenceEquals(TableTypeDto, null)) return 0;

            int hashTableTypeDtoName = TableTypeDto.Name == null ? 0 : TableTypeDto.Name.GetHashCode();

            int hashTableTypeDtoCharge = TableTypeDto.ChargePerSeat == null ? 0 : TableTypeDto.ChargePerSeat.GetHashCode();

            int hashTableTypeDtoCombine = TableTypeDto.CanBeCombined == null ? 0 : TableTypeDto.CanBeCombined.GetHashCode();

            int hashTableId = TableTypeDto.Id.GetHashCode();

            return hashTableTypeDtoName ^ hashTableTypeDtoCharge ^ hashTableTypeDtoCombine ^ hashTableId;
        }
    }
}

