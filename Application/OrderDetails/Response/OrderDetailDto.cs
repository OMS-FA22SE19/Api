using Application.Common.Mappings;
using AutoMapper;
using Core.Entities;
using Core.Enums;

namespace Application.OrderDetails.Response
{
    public sealed class OrderDetailDto : IMapFrom<OrderDetail>
    {
        public string OrderId { get; set; }
        public string UserId { get; set; }
        public DateTime Date { get; set; }
        public int FoodId { get; set; }
        public string FoodName { get; set; }
        public OrderDetailStatus Status { get; set; }
        public double Price { get; set; }
        public string Note { get; set; }
        public int Quantity { get; set; }
        public double Amount { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<OrderDetail, OrderDetailDto>()
                .ForMember(e => e.UserId, opt => opt.MapFrom(e => e.Order.UserId))
                .ForMember(e => e.Date, opt => opt.MapFrom(e => e.Order.Date.ToString("dd/MM/yyyy HH:mm:ss")))
                .ForMember(e => e.FoodName, opt => opt.MapFrom(e => e.Food.Name))
                .ReverseMap();
        }
    }
}
