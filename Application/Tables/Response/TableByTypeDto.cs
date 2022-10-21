using Application.Common.Mappings;
using AutoMapper;
using Core.Entities;

namespace Application.Tables.Response
{
    public class TableByTypeDto : IMapFrom<Table>, IEquatable<TableByTypeDto>
    {
        public int NumOfSeats { get; set; }
        public int TableTypeId { get; set; }
        public string TableTypeName { get; set; }
        public int Quantity { get; set; }

        public bool Equals(TableByTypeDto? other)
        {
            if (other is null)
            {
                return false;
            }
            return this.NumOfSeats == other.NumOfSeats
                && this.TableTypeId == other.TableTypeId
                && this.TableTypeName == other.TableTypeName
                && this.Quantity == other.Quantity;
        }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Table, TableByTypeDto>()
                .ForMember(dto => dto.TableTypeName, opt => opt.MapFrom(e => e.TableType.Name))
                .ReverseMap();
        }
    }

    public sealed class TableByTypeDtoComparer : IEqualityComparer<TableByTypeDto>
    {
        public bool Equals(TableByTypeDto x, TableByTypeDto y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.NumOfSeats == y.NumOfSeats
                && x.TableTypeId == y.TableTypeId
                && x.TableTypeName == y.TableTypeName
                && x.Quantity == y.Quantity;
        }

        public int GetHashCode(TableByTypeDto tableType)
        {
            if (Object.ReferenceEquals(tableType, null)) return 0;

            int hashNumOfSeats = tableType.NumOfSeats == null ? 0 : tableType.NumOfSeats.GetHashCode();

            int hashTableTypeId = tableType.TableTypeId == null ? 0 : tableType.TableTypeId.GetHashCode();

            int hashTableTypeName = tableType.TableTypeName == null ? 0 : tableType.TableTypeName.GetHashCode();

            int hashQuantity = tableType.Quantity == null ? 0 : tableType.Quantity.GetHashCode();

            return hashNumOfSeats ^ hashTableTypeId ^ hashTableTypeName ^ hashQuantity;
        }
    }
}
