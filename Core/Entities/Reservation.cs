using Core.Enums;
using Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public sealed class Reservation : BaseAuditableEntity, IEquatable<Reservation>
    {
        [Required]
        public string UserId { get; set; }
        [Range(1, 1000)]
        public int NumOfPeople { get; set; }
        public int TableTypeId { get; set; }
        public int NumOfSeats { get; set; }
        public int Quantity { get; set; }
        public int NumOfEdits { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public ReservationStatus Status { get; set; }
        public bool IsPriorFoodOrder { get; set; }
        public string? ReasonForCancel { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }

        public Order Order { get; set; }
        public ApplicationUser User { get; set; }
        public Billing Billing { get; set; }

        public IList<ReservationTable> ReservationTables { get; set; }

        public bool Equals(Reservation? other)
        {
            if (other is null)
            {
                return false;
            }
            if (other.ReservationTables == null)
            {
                return this.Id == other.Id
                    && this.UserId == other.UserId
                    && this.NumOfPeople == other.NumOfPeople
                    && this.TableTypeId == other.TableTypeId
                    && this.NumOfSeats == other.NumOfSeats
                    && this.Quantity == other.Quantity
                    && this.StartTime == other.StartTime
                    && this.EndTime == other.EndTime
                    && this.Status == other.Status
                    && this.IsPriorFoodOrder == other.IsPriorFoodOrder;
            }
            return this.Id == other.Id
                    && this.UserId == other.UserId
                    && this.NumOfPeople == other.NumOfPeople
                    && this.TableTypeId == other.TableTypeId
                    && this.NumOfSeats == other.NumOfSeats
                    && this.Quantity == other.Quantity
                    && this.StartTime == other.StartTime
                    && this.EndTime == other.EndTime
                    && this.Status == other.Status
                    && this.IsPriorFoodOrder == other.IsPriorFoodOrder
                    ;
        }
    }

    public sealed class ReservationComparer : IEqualityComparer<Reservation>
    {
        public bool Equals(Reservation x, Reservation y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.Id == y.Id
                && x.UserId == y.UserId
                && x.NumOfPeople == y.NumOfPeople
                && x.TableTypeId == y.TableTypeId
                && x.NumOfSeats == y.NumOfSeats
                && x.Quantity == y.Quantity
                && x.StartTime == y.StartTime
                && x.EndTime == y.EndTime
                && x.Status == y.Status
                && x.IsPriorFoodOrder == y.IsPriorFoodOrder;
        }

        public int GetHashCode(Reservation Reservation)
        {
            if (Object.ReferenceEquals(Reservation, null)) return 0;

            int hashReservationUserId = Reservation.UserId == null ? 0 : Reservation.UserId.GetHashCode();

            int hashReservationNumOfPeople = Reservation.NumOfPeople == null ? 0 : Reservation.NumOfPeople.GetHashCode();

            int hashReservationTableTypeId = Reservation.TableTypeId == null ? 0 : Reservation.TableTypeId.GetHashCode();

            int hashReservationNumOfSeats = Reservation.NumOfSeats == null ? 0 : Reservation.NumOfSeats.GetHashCode();

            int hashReservationQuantity = Reservation.Quantity == null ? 0 : Reservation.Quantity.GetHashCode();

            int hashReservationStartTime = Reservation.StartTime == null ? 0 : Reservation.StartTime.GetHashCode();

            int hashReservationEndTime = Reservation.EndTime == null ? 0 : Reservation.EndTime.GetHashCode();

            int hashReservationStatus = Reservation.Status == null ? 0 : Reservation.Status.GetHashCode();

            int hashReservationIsPriorFoodOrder = Reservation.IsPriorFoodOrder == null ? 0 : Reservation.IsPriorFoodOrder.GetHashCode();

            int hashMenuId = Reservation.Id.GetHashCode();

            return hashReservationUserId ^ hashReservationNumOfPeople ^ hashReservationTableTypeId ^ hashReservationNumOfSeats ^ hashReservationQuantity
                ^ hashReservationStartTime ^ hashReservationEndTime ^ hashReservationStatus ^ hashReservationIsPriorFoodOrder
                ^ hashMenuId;
        }
    }
}