using Application.Common.Interfaces;
using Application.Dashboard.Response;
using Application.Models;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using System.Linq.Expressions;

namespace Application.Dashboard.Queries
{
    public sealed class GetMonthlyOrderReservationQuery : IRequest<Response<ActiveOrdersReservations>>
    {

    }

    public sealed class GetMonthlyOrderReservationQueryHandler : IRequestHandler<GetMonthlyOrderReservationQuery, Response<ActiveOrdersReservations>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTime _dateTime;

        public GetMonthlyOrderReservationQueryHandler(IUnitOfWork unitOfWork, IDateTime dateTime)
        {
            _unitOfWork = unitOfWork;
            _dateTime = dateTime;
        }

        public async Task<Response<ActiveOrdersReservations>> Handle(GetMonthlyOrderReservationQuery request, CancellationToken cancellationToken)
        {
            var dateOfLastMonth = new DateTime(_dateTime.Now.Year, _dateTime.Now.Month - 1, DateTime.DaysInMonth(_dateTime.Now.Year, _dateTime.Now.Month - 1));

            List<Expression<Func<Reservation, bool>>> reservationFilters = new();
            reservationFilters.Add(e => !e.IsDeleted);
            reservationFilters.Add(e => e.Created > dateOfLastMonth && !(e.Status == ReservationStatus.Cancelled && e.Status == ReservationStatus.Available));
            var reservations = await _unitOfWork.ReservationRepository.GetAllAsync(reservationFilters);

            List<Expression<Func<Order, bool>>> orderFilters = new();
            orderFilters.Add(e => !e.IsDeleted);
            orderFilters.Add(e => e.Created > dateOfLastMonth && e.Status != OrderStatus.Reserved);
            var orders = await _unitOfWork.OrderRepository.GetAllAsync(orderFilters);

            var response = new ActiveOrdersReservations { Reservations = reservations.Count, Orders = orders.Count };

            return await Task.FromResult(new Response<ActiveOrdersReservations>(response));
        }
    }
}
