using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Mappings;
using Application.Models;
using Application.OrderDetails.Response;
using Application.Orders.Events;
using Application.Orders.Helpers;
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
    public sealed class AddNewDishesToOrderCommand : IMapFrom<Order>, IRequest<Response<OrderDto>>
    {
        public string OrderId { get; set; }
        public Dictionary<int, FoodInfo> OrderDetails { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<AddNewDishesToOrderCommand, Order>()
                .ForSourceMember(dto => dto.OrderDetails, opt => opt.DoNotValidate());
        }
    }

    public sealed class AddNewDishesToOrderCommandHandler : IRequestHandler<AddNewDishesToOrderCommand, Response<OrderDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IDateTime _dateTime;

        public AddNewDishesToOrderCommandHandler(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IMapper mapper, IDateTime dateTime)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
            _dateTime = dateTime;
        }

        public async Task<Response<OrderDto>> Handle(AddNewDishesToOrderCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(e => e.UserName.Equals("defaultCustomer"), cancellationToken);
            var availableMenu = await _unitOfWork.MenuRepository.GetAsync(e => e.Available);
            if (availableMenu is null)
            {
                throw new NotFoundException(nameof(Menu), $"No available {nameof(Menu)}");
            }
            var order = await _unitOfWork.OrderRepository.GetAsync(e => e.Id == request.OrderId && !e.IsDeleted && e.Status == OrderStatus.Processing, $"{nameof(Order.OrderDetails)}");
            if (order is null)
            {
                throw new NotFoundException(nameof(Order), request.OrderId);
            }

            foreach (var dish in request.OrderDetails)
            {
                for (int i = 0; i < dish.Value.Quantity; i++)
                {
                    var food = await _unitOfWork.MenuFoodRepository.GetAsync(e => e.FoodId == dish.Key && e.MenuId == availableMenu.Id);
                    if (food is null)
                    {
                        throw new NotFoundException(nameof(Food), dish.Key);
                    }
                    order.OrderDetails.Add(new OrderDetail
                    {
                        FoodId = dish.Key,
                        Price = food.Price,
                        Note = string.IsNullOrWhiteSpace(dish.Value.Note) ? string.Empty : dish.Value.Note,
                        Status = OrderDetailStatus.Received
                    });
                }
            }
            var result = await _unitOfWork.OrderRepository.UpdateAsync(order);
            order.AddDomainEvent(new AddNewDishesToOrderEvent
            {
                id = order.Id
            });
            await _unitOfWork.CompleteAsync(cancellationToken);
            if (result is null)
            {
                return new Response<OrderDto>("error");
            }

            var mappedResult = _mapper.Map<OrderDto>(result);
            mappedResult.FullName = user.FullName;
            mappedResult.PhoneNumber = user.PhoneNumber;

            var orderDetails = new List<OrderDetailDto>();
            double total = 0;
            foreach (var detail in result.OrderDetails)
            {
                var element = orderDetails.FirstOrDefault(e => e.FoodId.Equals(detail.FoodId) && e.Status.Equals(detail.Status));
                if (element is null)
                {
                    var food = await _unitOfWork.FoodRepository.GetAsync(e => e.Id == detail.FoodId);
                    orderDetails.Add(new OrderDetailDto
                    {
                        OrderId = result.Id,
                        FoodId = detail.FoodId,
                        FoodName = food.Name,
                        Status = detail.Status,
                        Quantity = 1,
                        Price = detail.Price,
                        Amount = detail.Price
                    });
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
