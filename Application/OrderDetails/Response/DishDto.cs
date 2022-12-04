using Application.Common.Mappings;
using Application.Orders.Response;
using AutoMapper;
using Core.Entities;
using Core.Enums;

namespace Application.OrderDetails.Response
{
    public class DishDto : IMapFrom<OrderDetail>, IEquatable<DishDto>
    {
        public int Id { get; set; }
        public string OrderId { get; set; }
        public string UserId { get; set; }
        public string PhoneNumber { get; set; }
        public int TableId { get; set; }
        public DateTime Date { get; set; }
        public int FoodId { get; set; }
        public string Note { get; set; }
        public string FoodName { get; set; }
        public double Price { get; set; }
        public OrderDetailStatus Status { get; set; }

        public void Mapping(Profile profile) => profile.CreateMap<OrderDetail, DishDto>()
                .ForMember(e => e.UserId, opt => opt.MapFrom(e => e.Order.UserId))
                .ForMember(e => e.Date, opt => opt.MapFrom(e => e.Created))
                .ForMember(e => e.FoodName, opt => opt.MapFrom(e => e.Food.Name))
                .ForMember(e => e.PhoneNumber, opt => opt.MapFrom(e => e.Order.User.PhoneNumber))
                .ReverseMap();

        public bool Equals(DishDto? other)
        {
            if (other is null)
            {
                return false;
            }

            return this.Id == other.Id
                && this.UserId == other.UserId
                && this.OrderId == other.OrderId
                && this.PhoneNumber == other.PhoneNumber
                && this.TableId == other.TableId
                && this.Date == other.Date
                && this.Status == other.Status
                && this.FoodId == other.FoodId
                && this.Note == other.Note
                && this.FoodName == other.FoodName
                && this.Price == other.Price;

        }
    }

    public sealed class DishDtoComparer : IEqualityComparer<DishDto>
    {
        public bool Equals(DishDto x, DishDto y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.Id == y.Id
                && x.UserId == y.UserId
                && x.OrderId == y.OrderId
                && x.PhoneNumber == y.PhoneNumber
                && x.TableId == y.TableId
                && x.Date == y.Date
                && x.Status == y.Status
                && x.FoodId == y.FoodId
                && x.Note == y.Note
                && x.FoodName == y.FoodName
                && x.Price == y.Price;
        }

        public int GetHashCode(DishDto DishDto)
        {
            if (Object.ReferenceEquals(DishDto, null)) return 0;

            int hashDishDtoUserId = DishDto.UserId == null ? 0 : DishDto.UserId.GetHashCode();

            int hashDishDtoTableId = DishDto.TableId == null ? 0 : DishDto.TableId.GetHashCode();

            int hashDishDtoOrderId = DishDto.OrderId == null ? 0 : DishDto.OrderId.GetHashCode();

            int hashDishDtoPhoneNumber = DishDto.PhoneNumber == null ? 0 : DishDto.PhoneNumber.GetHashCode();

            int hashDishDtoDate = DishDto.Date == null ? 0 : DishDto.Date.GetHashCode();

            int hashDishDtoStatus = DishDto.Status == null ? 0 : DishDto.Status.GetHashCode();

            int hashDishDtoFoodId = DishDto.FoodId == null ? 0 : DishDto.FoodId.GetHashCode();

            int hashDishDtoNote = DishDto.Note == null ? 0 : DishDto.Note.GetHashCode();

            int hashDishDtoFoodName = DishDto.FoodName == null ? 0 : DishDto.FoodName.GetHashCode();

            int hashDishDtoPrice = DishDto.Price == null ? 0 : DishDto.Price.GetHashCode();

            int hashDishId = DishDto.Id.GetHashCode();

            return hashDishDtoUserId ^ hashDishDtoTableId ^ hashDishDtoOrderId ^ hashDishDtoPhoneNumber ^ hashDishDtoDate ^ hashDishDtoStatus ^ hashDishDtoFoodId ^ hashDishDtoNote ^ hashDishDtoFoodName ^ hashDishDtoPrice ^ hashDishId;
        }
    }
}
