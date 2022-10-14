using Domain.Common;

namespace Core.Entities
{
    public sealed class TableType : BaseAuditableEntity
    {
        public string Name { get; set; }
        public double ChargePerSeat { get; set; }
        public bool CanBeCombined { get; set; }

        public IList<Table> Tables { get; set; }
    }
}
