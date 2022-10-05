using Application.Common.Mappings;
using Application.OrderDetails.Response;
using AutoMapper;
using Core.Entities;
using Core.Enums;

namespace Application.Orders.Response
{
    public sealed class OrderDto : IMapFrom<Order>
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Date { get; set; }
        public OrderStatus Status { get; set; }
        public double PrePaid { get; set; } = 0;
        public double Total { get; set; }

        public IList<OrderDetailDto> OrderDetails { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Order, OrderDto>()
                .ForMember(dto => dto.Date, opt => opt.MapFrom(e => e.Date.ToString("dd/MM/yyyy HH:mm:ss")))
                .ForMember(dto => dto.FullName, opt => opt.MapFrom(e => e.User.FullName))
                .ForMember(dto => dto.PhoneNumber, opt => opt.MapFrom(e => e.User.PhoneNumber))
                .ReverseMap();
        }
    }
}
