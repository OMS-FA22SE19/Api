using Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public sealed class ReservationTable : BaseAuditableEntity
    {
        [Required]
        public int ReservationId { get; set; }
        [Required]
        public int TableId { get; set; }

        public Reservation Reservation { get; set; }
        public Table Table { get; set; }
    }
}
