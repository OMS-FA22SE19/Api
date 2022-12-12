using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Models;
using Application.OrderDetails.Response;
using Application.Orders.Response;
using Application.Reservations.Response;
using AutoMapper;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Orders.Queries
{
    public sealed class GetOrderWithIdQuery : IRequest<Response<OrderDto>>
    {
        [Required]
        public string Id { get; init; }
    }

    public sealed class GetOrderWithIdQueryHandler : IRequestHandler<GetOrderWithIdQuery, Response<OrderDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public GetOrderWithIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<Response<OrderDto>> Handle(GetOrderWithIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _unitOfWork.OrderRepository.GetAsync(e => e.Id.Equals(request.Id), $"{nameof(Order.OrderDetails)}.{nameof(OrderDetail.Food)},{nameof(Order.Reservation)}");
            if (result is null)
            {
                throw new NotFoundException(nameof(Order), $"with {request.Id}");
            }

            if (_currentUserService.Role.Equals("Customer"))
            {
                if (!_currentUserService.UserName.Equals("defaultCustomer"))
                {
                    if (!_currentUserService.UserId.Equals(result.Reservation.UserId))
                    {
                        throw new BadRequestException("This is not your reservation");
                    }
                }
            }

            var mappedResult = _mapper.Map<OrderDto>(result);
            double total = 0;

            var reservation = await _unitOfWork.ReservationRepository.GetAsync(e => result.ReservationId == e.Id, $"{nameof(Reservation.ReservationTables)}");
            if (reservation.ReservationTables.Any())
            {
                mappedResult.TableId = reservation.ReservationTables.OrderBy(e => e.TableId).First().TableId;
            }

            List<OrderDetailDto> orderDetails = new();
            if (result.OrderDetails == null)
            {
                return new Response<OrderDto>(mappedResult);
            }
            foreach (var detail in result.OrderDetails)
            {
                if (detail.Status != OrderDetailStatus.Cancelled)
                {
                    var element = orderDetails.FirstOrDefault(e => e.FoodId.Equals(detail.FoodId));
                    if (element is null)
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
                        element.Quantity += 1;
                        element.Amount += detail.Price;
                    }
                    total += detail.Price;
                }
            }
            total -= result.PrePaid;

            mappedResult.OrderDetails = orderDetails;
            mappedResult.Total = total;
            mappedResult.Reservation = _mapper.Map<ReservationDto>(reservation);
            return new Response<OrderDto>(mappedResult);
        }
    }
}
