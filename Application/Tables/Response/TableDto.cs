using Application.Common.Mappings;
using Application.TableTypes.Response;
using AutoMapper;
using Core.Entities;
using Core.Enums;

namespace Application.Tables.Response
{
    public class TableDto : IMapFrom<Table>, IEquatable<TableDto>
    {
        public int Id { get; set; }
        public int NumOfSeats { get; set; }
        public TableStatus Status { get; set; }
        public bool IsDeleted { get; set; }
        public TableTypeDto TableType { get; set; }

        public bool Equals(TableDto? other)
        {
            if (other is null)
            {
                return false;
            }
            if (other.TableType == null)
            {
                return this.Id == other.Id
                && this.NumOfSeats == other.NumOfSeats
                && this.Status == other.Status;
            }
            return this.Id == other.Id
                && this.NumOfSeats == other.NumOfSeats
                && this.Status == other.Status
                && this.TableType.Equals(other.TableType);
        }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Table, TableDto>().ReverseMap();
        }
    }

    public sealed class TableDtoComparer : IEqualityComparer<TableDto>
    {
        public bool Equals(TableDto x, TableDto y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.Id == y.Id
                && x.NumOfSeats == y.NumOfSeats
                && x.Status == y.Status
                && x.TableType.Equals(y.TableType);
        }

        public int GetHashCode(TableDto table)
        {
            if (Object.ReferenceEquals(table, null)) return 0;

            int hashNumOfSeat = table.NumOfSeats == null ? 0 : table.NumOfSeats.GetHashCode();

            int hashStatus = table.Status == null ? 0 : table.Status.GetHashCode();

            int hashTableType = table.TableType == null ? 0 : table.TableType.GetHashCode();

            int hashTableId = table.Id.GetHashCode();

            return hashNumOfSeat ^ hashStatus ^ hashTableType ^ hashTableId;
        }
    }
}
