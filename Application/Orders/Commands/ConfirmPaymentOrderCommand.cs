using Application.Common.Exceptions;
using Application.Models;
using Application.OrderDetails.Response;
using Application.Orders.Response;
using AutoMapper;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using System.Linq.Expressions;

namespace Application.Orders.Commands
{
    public sealed class ConfirmPaymentOrderCommand : IRequest<Response<OrderDto>>
    {
        public string Id { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ConfirmPaymentOrderCommand, Order>();
        }
    }

    public sealed class ConfirmPaymentOrderCommandHandler : IRequestHandler<ConfirmPaymentOrderCommand, Response<OrderDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ConfirmPaymentOrderCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<OrderDto>> Handle(ConfirmPaymentOrderCommand request, CancellationToken cancellationToken)
        {
            var entity = await _unitOfWork.OrderRepository.GetAsync(e => e.Id == request.Id, $"{nameof(Order.OrderDetails)},{nameof(Order.User)}");
            if (entity is null)
            {
                throw new NotFoundException(nameof(Order), request.Id);
            }

            if (entity.OrderDetails.Any(e => e.Status != OrderDetailStatus.Served))
            {
                return new Response<OrderDto>($"Order {entity.Id} cannot be confirmed. Make sure all dishes have been served!");
            }

            MapToEntity(entity);
            var result = await _unitOfWork.OrderRepository.UpdateAsync(entity);
            await _unitOfWork.CompleteAsync(cancellationToken);

            List<Expression<Func<OrderDetail, bool>>> filters = new();
            filters.Add(e => e.OrderId.Equals(result.Id));
            var detailInDatabase = await _unitOfWork.OrderDetailRepository.GetAllAsync(filters, includeProperties: $"{nameof(OrderDetail.Food)}");
            if (result is null)
            {
                return new Response<OrderDto>("error");
            }

            double total = 0;
            List<OrderDetailDto> orderDetails = new();
            foreach (var detail in detailInDatabase)
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
                }
                else
                {
                    element.Quantity += 1;
                    element.Amount += detail.Price;
                }
                total += detail.Price;
            }
            total -= entity.PrePaid;

            var bill = new OrderDto()
            {
                Id = entity.Id,
                Date = entity.Date.ToString(),
                PhoneNumber = entity.User.PhoneNumber,
                OrderDetails = orderDetails,
                PrePaid = entity.PrePaid,
                Total = total
            };
            return new Response<OrderDto>(bill);
        }

        private static void MapToEntity(Order entity)
        {
            entity.Status = OrderStatus.Paid;
        }
    }
}
