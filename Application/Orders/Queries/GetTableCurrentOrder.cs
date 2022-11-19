using Application.Common.Exceptions;
using Application.Models;
using Application.OrderDetails.Response;
using Application.Orders.Response;
using AutoMapper;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace Application.Orders.Queries
{
    public sealed class GetTableCurrentOrderQuery : IRequest<Response<OrderDto>>
    {
        [Required]
        public int TableId { get; init; }
    }

    public sealed class GetTableCurrentOrderQueryHandler : IRequestHandler<GetTableCurrentOrderQuery, Response<OrderDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetTableCurrentOrderQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<OrderDto>> Handle(GetTableCurrentOrderQuery request, CancellationToken cancellationToken)
        {
            List<Expression<Func<Order, bool>>> filters = new();
            Func<IQueryable<Order>, IOrderedQueryable<Order>> orderBy = null;

            filters.Add(o => o.Status.Equals(OrderStatus.Processing));
            orderBy = o => o.OrderBy(or => or.Date);

            var result = await _unitOfWork.OrderRepository.GetAllAsync(filters, orderBy, $"{nameof(Order.OrderDetails)}.{nameof(OrderDetail.Food)},{nameof(Order.User)}");
            if (!result.Any())
            {
                throw new NotFoundException("There is no order for this table");
            }

            Order? tableOrder = null;
            foreach (var order in result.ToList())
            {
                var reservation = await _unitOfWork.ReservationRepository.GetAsync(r => r.Id == order.ReservationId, $"{nameof(Reservation.ReservationTables)}");
                if (reservation.ReservationTables.Any(rt => rt.TableId == request.TableId))
                {
                    tableOrder = order;
                }
            }
            if (tableOrder is null)
            {
                throw new NotFoundException("There is no order for this table");
            }

            var mappedResult = _mapper.Map<OrderDto>(tableOrder);
            double total = 0;

            mappedResult.TableId = request.TableId;

            List<OrderDetailDto> orderDetails = new();
            if (tableOrder.OrderDetails == null)
            {
                return new Response<OrderDto>(mappedResult);
            }
            foreach (var detail in tableOrder.OrderDetails)
            {
                var element = orderDetails.FirstOrDefault(e => e.FoodId.Equals(detail.FoodId) && !detail.IsDeleted && e.Status == detail.Status);
                if (element is null)
                {
                    if (detail.Status != OrderDetailStatus.Cancelled && detail.Status != OrderDetailStatus.Reserved)
                    {
                        orderDetails.Add(new OrderDetailDto
                        {
                            OrderId = tableOrder.Id,
                            UserId = tableOrder.UserId,
                            Date = tableOrder.Date,
                            FoodId = detail.FoodId,
                            FoodName = detail.Food.Name,
                            Status = detail.Status,
                            Quantity = 1,
                            Price = detail.Price,
                            Amount = detail.Price
                        });
                    }
                    else
                    {
                        orderDetails.Add(new OrderDetailDto
                        {
                            OrderId = tableOrder.Id,
                            UserId = tableOrder.UserId,
                            Date = tableOrder.Date,
                            FoodId = detail.FoodId,
                            FoodName = detail.Food.Name,
                            Status = detail.Status,
                            Quantity = 1,
                            Price = detail.Price,
                            Amount = 0
                        });
                    }
                }
                else
                {
                    element.Quantity += 1;
                    if (detail.Status != OrderDetailStatus.Cancelled && detail.Status != OrderDetailStatus.Reserved)
                    {
                        element.Amount += detail.Price;
                    }
                    else
                        element.Amount += 0;
                }
                if (detail.Status != OrderDetailStatus.Cancelled && detail.Status != OrderDetailStatus.Reserved)
                {
                    total += detail.Price;
                }
            }
            total -= tableOrder.PrePaid;

            mappedResult.OrderDetails = orderDetails;
            mappedResult.Total = total;
            return new Response<OrderDto>(mappedResult);
        }
    }
}
