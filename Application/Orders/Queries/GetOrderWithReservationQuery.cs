﻿using Application.Common.Exceptions;
using Application.Models;
using Application.OrderDetails.Response;
using Application.Orders.Response;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Orders.Queries
{
    public sealed class GetOrderWithReservationQuery : IRequest<Response<OrderDto>>
    {
        [Required]
        public int reservationId { get; init; }
    }

    public sealed class GetOrderWithReservationQueryHandler : IRequestHandler<GetOrderWithReservationQuery, Response<OrderDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetOrderWithReservationQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<OrderDto>> Handle(GetOrderWithReservationQuery request, CancellationToken cancellationToken)
        {
            var result = await _unitOfWork.OrderRepository.GetAsync(e => e.ReservationId.Equals(request.reservationId), $"{nameof(Order.OrderDetails)}.{nameof(OrderDetail.Food)},{nameof(Order.User)}");
            if (result is null)
            {
                throw new NotFoundException(nameof(Order), $"with Reservation Id {request.reservationId}");
            }
            var mappedResult = _mapper.Map<OrderDto>(result);
            double total = 0;

            List<OrderDetailDto> orderDetails = new();
            if (result.OrderDetails == null)
            {
                return new Response<OrderDto>(mappedResult);
            }
            foreach (var detail in result.OrderDetails)
            {
                var element = orderDetails.FirstOrDefault(e => e.FoodId.Equals(detail.FoodId));
                if (element is null)
                {
                    orderDetails.Add(new OrderDetailDto
                    {
                        OrderId = result.Id,
                        UserId = result.UserId,
                        Date = result.Date,
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
                    element.Quantity += 1;
                    element.Amount += detail.Price;
                }
                total += detail.Price;
            }
            total -= result.PrePaid;

            mappedResult.OrderDetails = orderDetails;
            mappedResult.Total = total;
            return new Response<OrderDto>(mappedResult);
        }
    }
}