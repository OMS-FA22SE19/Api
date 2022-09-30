using Domain.Common;

namespace Core.Entities
{
    public sealed class TableType : BaseEntity
    {
        public string Name { get; set; }
        public double ChargePerSeat { get; set; }

        public IList<Table> Tables { get; set; }
    }
}
