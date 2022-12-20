using Application.Common.Mappings;
using Application.OrderDetails.Response;
using Application.Users.Response;
using AutoMapper;
using Core.Entities;
using Core.Enums;

namespace Application.Reservations.Response
{
    public class ReservationDto : IMapFrom<Reservation>, IEquatable<ReservationDto>
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int NumOfPeople { get; set; }
        public int TableTypeId { get; set; }
        public string TableType { get; set; }
        public int NumOfSeats { get; set; }
        public int Quantity { get; set; }
        public double PrePaid { get; set; }
        public double Paid { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public ReservationStatus Status { get; set; }
        public bool IsPriorFoodOrder { get; set; }
        public DateTime Created { get; set; }
        public int NumOfEdits { get; set; }
        public string ReasonForCancel { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string TableId { get; set; }
        public IList<ReservationTableDto> ReservationTables { get; set; }
        public IList<OrderDetailDto> OrderDetails { get; set; }
        public UserDto User { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Reservation, ReservationDto>().ReverseMap();
        }
        public bool Equals(ReservationDto? other)
        {
            if (other is null)
            {
                return false;
            }
            if (other.ReservationTables == null)
            {
                if (other.OrderDetails == null)
                {
                    return this.Id == other.Id
                        && this.UserId == other.UserId
                        && this.NumOfPeople == other.NumOfPeople
                        && this.TableTypeId == other.TableTypeId
                        && this.TableType == other.TableType
                        && this.NumOfSeats == other.NumOfSeats
                        && this.Quantity == other.Quantity
                        && this.PrePaid == other.PrePaid
                        && this.Paid == other.Paid
                        && this.StartTime == other.StartTime
                        && this.EndTime == other.EndTime
                        && this.Status == other.Status
                        && this.IsPriorFoodOrder == other.IsPriorFoodOrder;
                }
                else
                {
                    return this.Id == other.Id
                        && this.UserId == other.UserId
                        && this.NumOfPeople == other.NumOfPeople
                        && this.TableTypeId == other.TableTypeId
                        && this.TableType == other.TableType
                        && this.NumOfSeats == other.NumOfSeats
                        && this.Quantity == other.Quantity
                        && this.PrePaid == other.PrePaid
                        && this.Paid == other.Paid
                        && this.StartTime == other.StartTime
                        && this.EndTime == other.EndTime
                        && this.Status == other.Status
                        && this.IsPriorFoodOrder == other.IsPriorFoodOrder;
                }
            }
            if (other.OrderDetails == null)
            {
                return this.Id == other.Id
                        && this.UserId == other.UserId
                        && this.NumOfPeople == other.NumOfPeople
                        && this.TableTypeId == other.TableTypeId
                        && this.TableType == other.TableType
                        && this.NumOfSeats == other.NumOfSeats
                        && this.Quantity == other.Quantity
                        && this.PrePaid == other.PrePaid
                        && this.Paid == other.Paid
                        && this.StartTime == other.StartTime
                        && this.EndTime == other.EndTime
                        && this.Status == other.Status
                        && this.IsPriorFoodOrder == other.IsPriorFoodOrder
                        && this.ReservationTables.SequenceEqual(other.ReservationTables, new ReservationTableComparer());
            }
            return this.Id == other.Id
                        && this.UserId == other.UserId
                        && this.NumOfPeople == other.NumOfPeople
                        && this.TableTypeId == other.TableTypeId
                        && this.TableType == other.TableType
                        && this.NumOfSeats == other.NumOfSeats
                        && this.Quantity == other.Quantity
                        && this.PrePaid == other.PrePaid
                        && this.Paid == other.Paid
                        && this.StartTime == other.StartTime
                        && this.EndTime == other.EndTime
                        && this.Status == other.Status
                        && this.IsPriorFoodOrder == other.IsPriorFoodOrder
                        && this.ReservationTables.SequenceEqual(other.ReservationTables, new ReservationTableComparer());
        }
    }

    public sealed class ReservationDtoComparer : IEqualityComparer<ReservationDto>
    {
        public bool Equals(ReservationDto x, ReservationDto y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.Id == y.Id
                && x.UserId == y.UserId
                && x.NumOfPeople == y.NumOfPeople
                && x.TableTypeId == y.TableTypeId
                && x.TableType == y.TableType
                && x.NumOfSeats == y.NumOfSeats
                && x.Quantity == y.Quantity
                && x.PrePaid == y.PrePaid
                && x.Paid == y.Paid
                && x.StartTime == y.StartTime
                && x.EndTime == y.EndTime
                && x.Status == y.Status
                && x.IsPriorFoodOrder == y.IsPriorFoodOrder;
        }

        public int GetHashCode(ReservationDto ReservationDto)
        {
            if (Object.ReferenceEquals(ReservationDto, null)) return 0;

            int hashReservationDtoUserId = ReservationDto.UserId == null ? 0 : ReservationDto.UserId.GetHashCode();

            int hashReservationDtoNumOfPeople = ReservationDto.NumOfPeople == null ? 0 : ReservationDto.NumOfPeople.GetHashCode();

            int hashReservationDtoTableTypeId = ReservationDto.TableTypeId == null ? 0 : ReservationDto.TableTypeId.GetHashCode();

            int hashReservationDtoTableType = ReservationDto.TableType == null ? 0 : ReservationDto.TableType.GetHashCode();

            int hashReservationDtoNumOfSeats = ReservationDto.NumOfSeats == null ? 0 : ReservationDto.NumOfSeats.GetHashCode();

            int hashReservationDtoQuantity = ReservationDto.Quantity == null ? 0 : ReservationDto.Quantity.GetHashCode();

            int hashReservationDtoPrePaid = ReservationDto.PrePaid == null ? 0 : ReservationDto.PrePaid.GetHashCode();

            int hashReservationDtoPaid = ReservationDto.Paid == null ? 0 : ReservationDto.Paid.GetHashCode();

            int hashReservationDtoStartTime = ReservationDto.StartTime == null ? 0 : ReservationDto.StartTime.GetHashCode();

            int hashReservationDtoEndTime = ReservationDto.EndTime == null ? 0 : ReservationDto.EndTime.GetHashCode();

            int hashReservationDtoStatus = ReservationDto.Status == null ? 0 : ReservationDto.Status.GetHashCode();

            int hashReservationDtoIsPriorFoodOrder = ReservationDto.IsPriorFoodOrder == null ? 0 : ReservationDto.IsPriorFoodOrder.GetHashCode();

            int hashMenuId = ReservationDto.Id.GetHashCode();

            return hashReservationDtoUserId ^ hashReservationDtoNumOfPeople ^ hashReservationDtoTableTypeId ^ hashReservationDtoTableType
                ^ hashReservationDtoNumOfSeats ^ hashReservationDtoQuantity ^ hashReservationDtoPrePaid ^ hashReservationDtoPaid
                ^ hashReservationDtoStartTime ^ hashReservationDtoEndTime ^ hashReservationDtoStatus ^ hashReservationDtoIsPriorFoodOrder
                ^ hashMenuId;
        }
    }
}