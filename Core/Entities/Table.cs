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
        public TableType Type { get; set; }

        public IList<Reservation> Reservations { get; set; }
    }
}
