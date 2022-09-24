using Application.Models;
using Application.OrderDetails.Response;
using Application.Orders.Response;
using AutoMapper;
using Core.Common;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using System.Linq.Expressions;

namespace Application.Orders.Queries
{
    public sealed class GetOrderWithPaginationQuery : PaginationRequest, IRequest<Response<PaginatedList<OrderDto>>>
    {
        public new OrderProperty? OrderBy { get; init; }
        public OrderStatus? Status { get; init; }
    }

    public sealed class GetOrderWithPaginationQueryHandler : IRequestHandler<GetOrderWithPaginationQuery, Response<PaginatedList<OrderDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetOrderWithPaginationQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<PaginatedList<OrderDto>>> Handle(GetOrderWithPaginationQuery request, CancellationToken cancellationToken)
        {
            List<Expression<Func<Order, bool>>> filters = new();
            Func<IQueryable<Order>, IOrderedQueryable<Order>> orderBy = null;
            string includeProperties = $"{nameof(Order.OrderDetails)}.{nameof(OrderDetail.Food)},{nameof(Order.User)}";

            if (!string.IsNullOrWhiteSpace(request.SearchValue))
            {
                filters.Add(e => e.Id.Contains(request.SearchValue)
                    || e.UserId.Contains(request.SearchValue)
                    || e.Date.ToString().Contains(request.SearchValue));
            }
            if (request.Status != null)
            {
                filters.Add(e => e.Status == request.Status);
            }

            filters.Add(e => e.Date > DateTime.UtcNow.AddHours(7).AddHours(-3));

            switch (request.OrderBy)
            {
                case (OrderProperty.Id):
                    if (request.IsDescending)
                    {
                        orderBy = e => e.OrderByDescending(x => x.Id);
                        break;
                    }
                    orderBy = e => e.OrderBy(x => x.Id);
                    break;
                case (OrderProperty.UserId):
                    if (request.IsDescending)
                    {
                        orderBy = e => e.OrderByDescending(x => x.UserId);
                        break;
                    }
                    orderBy = e => e.OrderBy(x => x.UserId);
                    break;
                case (OrderProperty.Status):
                    if (request.IsDescending)
                    {
                        orderBy = e => e.OrderByDescending(x => x.Status);
                        break;
                    }
                    orderBy = e => e.OrderBy(x => x.Status);
                    break;
                case (OrderProperty.PrePaid):
                    if (request.IsDescending)
                    {
                        orderBy = e => e.OrderByDescending(x => x.PrePaid);
                        break;
                    }
                    orderBy = e => e.OrderBy(x => x.PrePaid);
                    break;
                default:
                    break;
            }

            var result = await _unitOfWork.OrderRepository.GetPaginatedListAsync(filters, orderBy, includeProperties, request.PageIndex, request.PageSize);
            var orderDtos = new List<OrderDto>();
            foreach (var order in result)
            {
                double total = 0;
                List<OrderDetailDto> orderDetails = new();
                var orderDto = new OrderDto
                {
                    Id = order.Id,
                    UserId = order.UserId,
                    UserName = order.User.UserName,
                    PhoneNumber = order.User.PhoneNumber,
                    Date = order.Date.ToString("dd/MM/yyyy HH:mm:ss"),
                    Status = order.Status,
                    PrePaid = order.PrePaid
                };

                foreach (var detail in order.OrderDetails)
                {
                    var element = orderDetails.FirstOrDefault(e => e.FoodId.Equals(detail.FoodId));
                    if (element is null)
                    {
                        orderDetails.Add(new OrderDetailDto
                        {
                            OrderId = order.Id,
                            UserId = order.UserId,
                            Date = order.Date.ToString("dd/MM/yyyy HH:mm:ss"),
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

                total -= order.PrePaid;
                orderDto.Total = total;
                orderDto.OrderDetails = orderDetails;

                orderDtos.Add(orderDto);
            }

            var mappedResult = new PaginatedList<OrderDto>(result.Count, result.TotalPages, result.TotalCount, orderDtos);
            return new Response<PaginatedList<OrderDto>>(mappedResult);
        }
    }
}
