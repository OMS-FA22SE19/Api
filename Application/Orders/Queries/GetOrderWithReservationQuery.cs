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
            var result = await _unitOfWork.OrderRepository.GetAsync(e => e.ReservationId.Equals(request.reservationId), $"{nameof(Order.OrderDetails)}.{nameof(OrderDetail.Food)},{nameof(Order.Reservation)}");
            if (result is null)
            {
                throw new NotFoundException(nameof(Order), $"with Reservation Id {request.reservationId}");
            }
            var mappedResult = _mapper.Map<OrderDto>(result);
            double total = 0;

            var reservation = await _unitOfWork.ReservationRepository.GetAsync(e => result.ReservationId == e.Id, $"{nameof(Reservation.ReservationTables)}");
            var tableType = await _unitOfWork.TableTypeRepository.GetAsync(e => e.Id == reservation.TableTypeId);
            if (reservation.ReservationTables.Any())
            {
                mappedResult.TableId = tableType.Name + " - " + reservation.ReservationTables.Min(e => e.TableId);
            }

            List<OrderDetailDto> orderDetails = new();
            if (result.OrderDetails == null)
            {
                return new Response<OrderDto>(mappedResult);
            }
            foreach (var detail in result.OrderDetails)
            {
                var element = orderDetails.FirstOrDefault(e => e.FoodId.Equals(detail.FoodId) && e.Status == detail.Status);
                if (element is null)
                {
                    if (detail.Status != OrderDetailStatus.Cancelled && detail.Status != OrderDetailStatus.Reserved)
                    {
                        orderDetails.Add(new OrderDetailDto
                        {
                            OrderId = result.Id,
                            UserId = result.Reservation.UserId,
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
                        orderDetails.Add(new OrderDetailDto
                        {
                            OrderId = result.Id,
                            UserId = result.Reservation.UserId,
                            Date = result.Date,
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
            total -= result.PrePaid;

            mappedResult.OrderDetails = orderDetails;
            mappedResult.Total = total;
            return new Response<OrderDto>(mappedResult);
        }
    }
}
