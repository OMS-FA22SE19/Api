using Core.Enums;
using Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public sealed class Table : BaseAuditableEntity, IEquatable<Table>
    {
        [Range(1, int.MaxValue)]
        public int NumOfSeats { get; set; }
        public TableStatus Status { get; set; }
        public int TableTypeId { get; set; }

        public IList<ReservationTable> ReservationsTables { get; set; }
        public TableType TableType { get; set; }

        public bool Equals(Table? other)
        {
            if (other is null)
            {
                return false;
            }
            return this.Id == other.Id
                && this.NumOfSeats == other.NumOfSeats
                && this.Status == other.Status
                && this.TableTypeId == other.TableTypeId;
        }
    }

    public sealed class TableComparer : IEqualityComparer<Table>
    {
        public bool Equals(Table x, Table y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.Id == y.Id
                && x.NumOfSeats == y.NumOfSeats
                && x.Status == y.Status
                && x.TableTypeId == y.TableTypeId;
        }

        public int GetHashCode(Table tableType)
        {
            if (Object.ReferenceEquals(tableType, null)) return 0;

            int hashtableTypeName = tableType.NumOfSeats == null ? 0 : tableType.NumOfSeats.GetHashCode();

            int hashTableTypeCharge = tableType.Status == null ? 0 : tableType.Status.GetHashCode();

            int hashTableTypeCombine = tableType.TableTypeId == null ? 0 : tableType.TableTypeId.GetHashCode();

            int hashTableId = tableType.Id.GetHashCode();

            return hashtableTypeName ^ hashTableTypeCharge ^ hashTableTypeCombine ^ hashTableId;
        }
    }
}
