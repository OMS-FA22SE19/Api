using Domain.Common;

namespace Core.Entities
{
    public sealed class TableType : BaseAuditableEntity, IEquatable<TableType>
    {
        public string Name { get; set; }
        public double ChargePerSeat { get; set; }
        public bool CanBeCombined { get; set; }

        public IList<Table> Tables { get; set; }

        public bool Equals(TableType? other)
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
    }

    public sealed class TableTypeComparer : IEqualityComparer<TableType>
    {
        public bool Equals(TableType x, TableType y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.Id == y.Id 
                && x.Name == y.Name
                && x.ChargePerSeat == y.ChargePerSeat
                && x.CanBeCombined == y.CanBeCombined;
        }

        public int GetHashCode(TableType tableType)
        {
            if (Object.ReferenceEquals(tableType, null)) return 0;

            int hashtableTypeName = tableType.Name == null ? 0 : tableType.Name.GetHashCode();

            int hashTableTypeCharge = tableType.ChargePerSeat == null ? 0 : tableType.ChargePerSeat.GetHashCode();

            int hashTableTypeCombine = tableType.CanBeCombined == null ? 0 : tableType.CanBeCombined.GetHashCode();

            int hashTableId = tableType.Id.GetHashCode();

            return hashtableTypeName ^ hashTableTypeCharge ^ hashTableTypeCombine ^ hashTableId;
        }
    }
}
