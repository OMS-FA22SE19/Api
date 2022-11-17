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
using System.Linq;
using System.Linq.Expressions;

namespace Application.Reservations.Queries
{
    public class GetReservationWithPaginationQuery : PaginationRequest, IRequest<Response<PaginatedList<ReservationDto>>>
    {
        public string? userId { get; set; }
        public ReservationStatus? Status { get; init; }
    }

    public class GetReservationWithPaginationQueryHandler : IRequestHandler<GetReservationWithPaginationQuery, Response<PaginatedList<ReservationDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetReservationWithPaginationQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<PaginatedList<ReservationDto>>> Handle(GetReservationWithPaginationQuery request, CancellationToken cancellationToken)
        {
            List<Expression<Func<Reservation, bool>>> filters = new();
            Func<IQueryable<Reservation>, IOrderedQueryable<Reservation>> orderBy = null;
            string includeProperties = $"{nameof(Reservation.User)},{nameof(Reservation.ReservationTables)}.{nameof(ReservationTable.Table)}.{nameof(Table.TableType)}";

            if (!string.IsNullOrWhiteSpace(request.userId))
            {
                filters.Add(e => e.UserId.Contains(request.userId));
            }
            if (request.Status is not null)
            {
                filters.Add(e => e.Status == request.Status);
            }

            var result = await _unitOfWork.ReservationRepository.GetPaginatedListAsync(filters, orderBy, includeProperties, request.PageIndex, request.PageSize);
            var mappedResult = _mapper.Map<PaginatedList<Reservation>, PaginatedList<ReservationDto>>(result);
            foreach (var item in mappedResult)
            {
                var tableType = await _unitOfWork.TableTypeRepository.GetAsync(e => e.Id == item.TableTypeId);
                if (tableType is null)
                {
                    throw new NotFoundException(nameof(TableType), item.TableTypeId);
                }
                var orderDetails = new List<OrderDetailDto>();

                var order = await _unitOfWork.OrderRepository.GetAsync(o => o.ReservationId == item.Id, $"{nameof(Order.OrderDetails)}");
                if (order is not null)
                {
                    foreach (var detail in order.OrderDetails)
                    {
                        var element = orderDetails.FirstOrDefault(e => e.FoodId.Equals(detail.FoodId) && e.Status == detail.Status);
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
                var billing = await _unitOfWork.BillingRepository.GetAsync(b => b.ReservationId == item.Id);
                if (billing is not null)
                {
                    item.Paid = billing.ReservationAmount;
                }

                item.OrderDetails = orderDetails;
                item.PrePaid = item.NumOfSeats * tableType.ChargePerSeat;
                item.TableType = tableType.Name;
            }
            return new Response<PaginatedList<ReservationDto>>(mappedResult);
        }
    }
}
