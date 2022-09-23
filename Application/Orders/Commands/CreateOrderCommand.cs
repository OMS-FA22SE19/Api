using Application.Common.Mappings;
using Application.Models;
using Application.OrderDetails.Response;
using Application.Orders.Response;
using AutoMapper;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.Commands
{
    public sealed class CreateOrderCommand : IMapFrom<Order>, IRequest<Response<OrderDto>>
    {
        public int TableId { get; set; }
        public Dictionary<int, int> OrderDetails { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<CreateOrderCommand, Order>()
                .ForSourceMember(dto => dto.OrderDetails, opt => opt.DoNotValidate());
        }
    }

    public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Response<OrderDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public CreateOrderCommandHandler(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<Response<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(e => e.UserName.Equals("defaultCustomer"), cancellationToken);
            var availableMenu = await _unitOfWork.MenuRepository.GetAsync(e => !e.IsHidden);

            var entity = new Order
            {
                Id = $"{request.TableId}-{user.PhoneNumber}-{DateTime.UtcNow.AddHours(7).ToString("dd-MM-yyyy-HH:mm:ss")}",
                UserId = user.Id,
                Date = DateTime.UtcNow.AddHours(7),
                Status = OrderStatus.Processing,
                OrderDetails = new List<OrderDetail>()
            };

            foreach (var dish in request.OrderDetails)
            {
                for (int i = 0; i < dish.Value; i++)
                {
                    var price = (await _unitOfWork.MenuFoodRepository.GetAsync(e => e.FoodId == dish.Key && e.MenuId == availableMenu.Id)).Price;
                    entity.OrderDetails.Add(new OrderDetail
                    {
                        FoodId = dish.Key,
                        Price = price,
                        Status = OrderDetailStatus.Received
                    });
                }
            }
            var result = await _unitOfWork.OrderRepository.InsertAsync(entity);
            await _unitOfWork.CompleteAsync(cancellationToken);
            if (result is null)
            {
                return new Response<OrderDto>("error");
            }

            var mappedResult = _mapper.Map<OrderDto>(result);
            mappedResult.PhoneNumber = user.PhoneNumber;

            var orderDetails = new List<OrderDetailDto>();
            double total = 0;
            foreach (var detail in result.OrderDetails)
            {
                var element = orderDetails.FirstOrDefault(e => e.FoodId.Equals(detail.FoodId));
                if (element is null)
                {
                    orderDetails.Add(new OrderDetailDto
                    {
                        OrderId = result.Id,
                        FoodId = detail.FoodId,
                        FoodName = detail.Food.Name,
                        Status = OrderDetailStatus.Served,
                        Quantity = 1,
                        Price = detail.Price,
                        Amount = detail.Price
                    });
                    entity.OrderDetails.Remove(detail);
                }
                else
                {
                    element.Quantity += 1;
                    element.Amount += detail.Price;
                }
                total += detail.Price;
            }
            total -= result.PrePaid;

            mappedResult.OrderDetails = orderDetails;
            return new Response<OrderDto>(mappedResult)
            {
                StatusCode = System.Net.HttpStatusCode.Created
            };
        }
    }
}
