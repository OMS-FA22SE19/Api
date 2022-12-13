using Application.Common.Mappings;
using Application.OrderDetails.Response;
using Application.Reservations.Response;
using AutoMapper;
using Core.Entities;
using Core.Enums;

namespace Application.Orders.Response
{
    public sealed class OrderDto : IMapFrom<Order>, IEquatable<OrderDto>
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string TableId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime Date { get; set; }
        public OrderStatus Status { get; set; }
        public double PrePaid { get; set; } = 0;
        public double Amount { get; set; }
        public double Total { get; set; }

        public IList<OrderDetailDto> OrderDetails { get; set; }
        public ReservationDto Reservation { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Order, OrderDto>()
                .ForMember(dto => dto.FullName, opt => opt.MapFrom(e => e.Reservation.FullName))
                .ForMember(dto => dto.PhoneNumber, opt => opt.MapFrom(e => e.Reservation.PhoneNumber))
                .ReverseMap();
        }

        public bool Equals(OrderDto? other)
        {
            if (other is null)
            {
                return false;
            }

            return this.Id == other.Id
                && this.UserId == other.UserId
                && this.TableId == other.TableId
                && this.FullName == other.FullName
                && this.PhoneNumber == other.PhoneNumber
                && this.Date == other.Date
                && this.Status == other.Status
                && this.PrePaid == other.PrePaid
                && this.Amount == other.Amount
                && this.Total == other.Total;

        }
    }

    public sealed class OrderDtoComparer : IEqualityComparer<OrderDto>
    {
        public bool Equals(OrderDto x, OrderDto y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.Id == y.Id
                && x.UserId == y.UserId
                && x.TableId == y.TableId
                && x.FullName == y.FullName
                && x.PhoneNumber == y.PhoneNumber
                && x.Date == y.Date
                && x.Status == y.Status
                && x.PrePaid == y.PrePaid
                && x.Amount == y.Amount
                && x.Total == y.Total;
        }

        public int GetHashCode(OrderDto OrderDto)
        {
            if (Object.ReferenceEquals(OrderDto, null)) return 0;

            int hashOrderDtoUserId = OrderDto.UserId == null ? 0 : OrderDto.UserId.GetHashCode();

            int hashOrderDtoTableId = OrderDto.TableId == null ? 0 : OrderDto.TableId.GetHashCode();

            int hashOrderDtoFullName = OrderDto.FullName == null ? 0 : OrderDto.FullName.GetHashCode();

            int hashOrderDtoPhoneNumber = OrderDto.PhoneNumber == null ? 0 : OrderDto.PhoneNumber.GetHashCode();

            int hashOrderDtoDate = OrderDto.Date == null ? 0 : OrderDto.Date.GetHashCode();

            int hashOrderDtoStatus = OrderDto.Status == null ? 0 : OrderDto.Status.GetHashCode();

            int hashOrderDtoPrePaid = OrderDto.PrePaid == null ? 0 : OrderDto.PrePaid.GetHashCode();

            int hashOrderDtoAmount = OrderDto.Amount == null ? 0 : OrderDto.Amount.GetHashCode();

            int hashOrderDtoTotal = OrderDto.Total == null ? 0 : OrderDto.Total.GetHashCode();

            int hashMenuId = OrderDto.Id.GetHashCode();

            return hashOrderDtoUserId ^ hashOrderDtoTableId ^ hashOrderDtoFullName ^ hashOrderDtoPhoneNumber ^ hashOrderDtoDate ^ hashOrderDtoStatus ^ hashOrderDtoPrePaid ^ hashOrderDtoAmount ^ hashMenuId;
        }
    }
}
