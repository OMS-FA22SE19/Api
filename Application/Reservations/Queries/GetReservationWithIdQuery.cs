using Application.Common.Exceptions;
using Application.Models;
using Application.OrderDetails.Response;
using Application.Reservations.Response;
using AutoMapper;
using Core.Common;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Reservations.Queries
{
    public class GetReservationWithIdQuery : IRequest<Response<ReservationDto>>
    {
        [Required]
        public int Id { get; init; }
    }

    public class GetReservationWithIdQueryHandler : IRequestHandler<GetReservationWithIdQuery, Response<ReservationDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetReservationWithIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<ReservationDto>> Handle(GetReservationWithIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _unitOfWork.ReservationRepository.GetAsync(e => e.Id == request.Id, $"{nameof(Reservation.ReservationTables)}.{nameof(ReservationTable.Table)}.{nameof(Table.TableType)},{nameof(Reservation.User)}");
            if (result is null)
            {
                throw new NotFoundException(nameof(Reservation), $"with {request.Id}");
            }
            var tableType = await _unitOfWork.TableTypeRepository.GetAsync(e => e.Id == result.TableTypeId);
            if (tableType is null)
            {
                throw new NotFoundException(nameof(TableType), result.TableTypeId);
            }

            var mappedResult = _mapper.Map<ReservationDto>(result);
            var orderDetails = new List<OrderDetailDto>();
            if (result.IsPriorFoodOrder)
            {
                var order = await _unitOfWork.OrderRepository.GetAsync(o => o.ReservationId == request.Id, $"{nameof(Order.OrderDetails)}");
                if (order is null)
                {
                    mappedResult.OrderDetails = orderDetails;
                    mappedResult.PrePaid = result.NumOfSeats * tableType.ChargePerSeat * result.Quantity;
                    mappedResult.TableType = tableType.Name;
                    return new Response<ReservationDto>(mappedResult);
                }
                foreach (var detail in order.OrderDetails)
                {
                    var element = orderDetails.FirstOrDefault(e => e.FoodId.Equals(detail.FoodId));
                    if (element is null)
                    {
                        var food = await _unitOfWork.FoodRepository.GetAsync(e => e.Id == detail.FoodId);
                        orderDetails.Add(new OrderDetailDto
                        {
                            OrderId = order.Id,
                            FoodId = detail.FoodId,
                            FoodName = food.Name,
                            Status = detail.Status,
                            Quantity = 1,
                            Price = detail.Price,
                            Amount = detail.Price,
                            Note= detail.Note,
                        });
                    }
                    else
                    {
                        element.Quantity += 1;
                        element.Amount += detail.Price;
                    }
                }
            }
            var billing = await _unitOfWork.BillingRepository.GetAsync(b => b.ReservationId == request.Id);
            if (billing is not null)
            {
                mappedResult.Paid = billing.ReservationAmount;
            }

            mappedResult.OrderDetails = orderDetails;
            mappedResult.PrePaid = result.NumOfSeats * tableType.ChargePerSeat * result.Quantity;
            mappedResult.TableType = tableType.Name;
            return new Response<ReservationDto>(mappedResult);
        }
    }
}
