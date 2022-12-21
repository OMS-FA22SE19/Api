using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Models;
using Application.OrderDetails.Response;
using Application.Reservations.Response;
using AutoMapper;
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
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTime _dateTime;

        public GetReservationWithIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService, IDateTime dateTime)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _dateTime = dateTime;
        }

        public async Task<Response<ReservationDto>> Handle(GetReservationWithIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _unitOfWork.ReservationRepository.GetAsync(e => e.Id == request.Id, $"{nameof(Reservation.ReservationTables)}.{nameof(ReservationTable.Table)}.{nameof(Table.TableType)},{nameof(Reservation.User)}");
            if (result is null)
            {
                throw new NotFoundException(nameof(Reservation), $"with {request.Id}");
            }


            if (result.Created < _dateTime.Now.AddMinutes(-15) && result.Status == ReservationStatus.Available)
            {
                var bill = await _unitOfWork.BillingRepository.GetAsync(e => e.ReservationId == result.Id);
                if (bill is null)
                {
                    result.Status = ReservationStatus.Cancelled;
                    result.ReasonForCancel = "Reservation is have not Paid 15 minutes before create";
                    await _unitOfWork.ReservationRepository.UpdateAsync(result);
                    await _unitOfWork.CompleteAsync(cancellationToken);
                }
            }

            if (_currentUserService.UserId is null)
            {
                throw new BadRequestException("You have to log in");
            }
            if (_currentUserService.Role.Equals("Customer"))
            {
                if (!_currentUserService.UserName.Equals("defaultCustomer"))
                {
                    if (!_currentUserService.UserId.Equals(result.UserId))
                    {
                        throw new BadRequestException("This is not your reservation");
                    }
                }
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
                            Note = detail.Note,
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
            if (mappedResult.ReservationTables?.Any() == true)
            {
                mappedResult.TableId = $"{tableType.Name} - {mappedResult.ReservationTables.Min(e => e.TableId)}";
            }
            return new Response<ReservationDto>(mappedResult);
        }
    }
}
