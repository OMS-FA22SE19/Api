﻿using Application.Common.Interfaces;
using Application.Models;
using Application.OrderDetails.Response;
using Application.Orders.Response;
using Application.Reservations.Response;
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
        public OrderProperty? SearchBy { get; init; }
        public OrderProperty? OrderBy { get; init; }
        public OrderStatus? Status { get; init; }
    }

    public sealed class GetOrderWithPaginationQueryHandler : IRequestHandler<GetOrderWithPaginationQuery, Response<PaginatedList<OrderDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public GetOrderWithPaginationQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<Response<PaginatedList<OrderDto>>> Handle(GetOrderWithPaginationQuery request, CancellationToken cancellationToken)
        {
            List<Expression<Func<Order, bool>>> filters = new();
            Func<IQueryable<Order>, IOrderedQueryable<Order>> orderBy = null;
            string includeProperties = $"{nameof(Order.OrderDetails)}.{nameof(OrderDetail.Food)},{nameof(Order.Reservation)}";

            if (!string.IsNullOrWhiteSpace(request.SearchValue))
            {
                switch (request.SearchBy)
                {
                    case OrderProperty.TableId:
                        if (int.TryParse(request.SearchValue, out int tableId))
                        {
                            filters.Add(e => e.Reservation.ReservationTables.Min(x => x.TableId) == tableId);
                        }
                        else
                        {
                            filters.Add(e => false);
                        }
                        break;
                    case OrderProperty.User:
                        filters.Add(e => e.Reservation.FullName.Contains(request.SearchValue));
                        break;
                    case OrderProperty.PhoneNumber:
                        filters.Add(e => e.Reservation.PhoneNumber.Contains(request.SearchValue));
                        break;
                    default:
                        break;
                }
            }
            if (request.Status != null)
            {
                filters.Add(e => e.Status == request.Status);
            }

            if (_currentUserService.Role.Equals("Customer"))
            {
                if (!_currentUserService.UserName.Equals("defaultCustomer"))
                {
                    filters.Add(e => e.Reservation.UserId.Equals(_currentUserService.UserId));
                }
            }

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
                        orderBy = e => e.OrderByDescending(x => x.Reservation.UserId);
                        break;
                    }
                    orderBy = e => e.OrderBy(x => x.Reservation.UserId);
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
                    UserId = order.Reservation.UserId,
                    FullName = order.Reservation.FullName,
                    PhoneNumber = order.Reservation.PhoneNumber,
                    Date = order.Date,
                    Status = order.Status,
                    PrePaid = order.PrePaid
                };

                var reservation = await _unitOfWork.ReservationRepository.GetAsync(e => order.ReservationId == e.Id, $"{nameof(Reservation.ReservationTables)}");
                var tableType = await _unitOfWork.TableTypeRepository.GetAsync(e => e.Id == reservation.TableTypeId);
                if (reservation.ReservationTables.Any())
                {
                    orderDto.TableId = tableType.Name + " - " + reservation.ReservationTables.Min(e => e.TableId);
                }


                foreach (var detail in order.OrderDetails)
                {
                    var element = orderDetails.FirstOrDefault(e => e.FoodId.Equals(detail.FoodId) && e.Status == detail.Status);
                    if (element is null)
                    {
                        if (detail.Status != OrderDetailStatus.Cancelled && detail.Status != OrderDetailStatus.Reserved)
                        {
                            orderDetails.Add(new OrderDetailDto
                            {
                                OrderId = order.Id,
                                UserId = order.Reservation.UserId,
                                Date = order.Date,
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
                                OrderId = order.Id,
                                UserId = order.Reservation.UserId,
                                Date = order.Date,
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

                orderDto.Amount = total;
                total -= order.PrePaid;
                orderDto.Total = total;
                orderDto.OrderDetails = orderDetails;
                orderDto.Reservation = _mapper.Map<ReservationDto>(reservation);

                orderDtos.Add(orderDto);
            }

            var mappedResult = new PaginatedList<OrderDto>(result.Count, result.TotalPages, result.TotalCount, orderDtos);
            return new Response<PaginatedList<OrderDto>>(mappedResult);
        }
    }
}
