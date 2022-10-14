using Core.Enums;
using Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public sealed class Table : BaseAuditableEntity
    {
        [Range(1, int.MaxValue)]
        public int NumOfSeats { get; set; }
        public TableStatus Status { get; set; }
        public int TableTypeId { get; set; }

        public IList<ReservationTable> ReservationsTables { get; set; }
        public TableType TableType { get; set; }
    }
}
