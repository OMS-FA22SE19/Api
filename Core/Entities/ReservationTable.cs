using Core.Common;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public sealed class ReservationTable : Entity, IEquatable<ReservationTable>
    {
        [Required]
        public int ReservationId { get; set; }
        [Required]
        public int TableId { get; set; }

        public Reservation Reservation { get; set; }
        public Table Table { get; set; }

        public bool Equals(ReservationTable? other)
        {
            if (other is null)
            {
                return false;
            }
            return this.ReservationId == other.ReservationId
                && this.TableId == other.TableId;
        }
    }

    public sealed class ReservationTableComparer : IEqualityComparer<ReservationTable>
    {
        public bool Equals(ReservationTable x, ReservationTable y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.ReservationId == y.ReservationId
                && x.TableId == y.TableId;
        }

        public int GetHashCode(ReservationTable ReservationTable)
        {
            if (Object.ReferenceEquals(ReservationTable, null)) return 0;

            int hashReservationTableReservationId = ReservationTable.ReservationId.GetHashCode();

            int hashReservationTableId = ReservationTable.TableId.GetHashCode();


            return hashReservationTableReservationId ^ hashReservationTableId;
        }
    }
}
