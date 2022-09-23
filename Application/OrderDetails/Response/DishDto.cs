using Application.Common.Mappings;
using AutoMapper;
using Core.Entities;
using Core.Enums;

namespace Application.OrderDetails.Response
{
    public class DishDto : IMapFrom<OrderDetail>
    {
        public int Id { get; set; }
        public string OrderId { get; set; }
        public string UserId { get; set; }
        public string Date { get; set; }
        public int FoodId { get; set; }
        public string FoodName { get; set; }
        public double Price { get; set; }
        public OrderDetailStatus Status { get; set; }

        public void Mapping(Profile profile) => profile.CreateMap<OrderDetail, DishDto>()
                .ForMember(e => e.UserId, opt => opt.MapFrom(e => e.Order.UserId))
                .ForMember(e => e.FoodName, opt => opt.MapFrom(e => e.Food.Name))
                .ForMember(e => e.Date, opt => opt.MapFrom(e => e.Order.Date.ToString("dd/MM/yyyy HH:mm:ss")))
                .ReverseMap();
    }
}
