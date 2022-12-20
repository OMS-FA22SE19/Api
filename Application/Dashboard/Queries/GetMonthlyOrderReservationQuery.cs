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

            List<Expression<Func<Reservation, bool>>> reservationFilters = new()
            {
                e => !e.IsDeleted,
                e => e.Status != ReservationStatus.Cancelled
            };
            var reservations = await _unitOfWork.ReservationRepository.GetAllAsync(reservationFilters);
            var lastMonthReservation = reservations.Count(e => e.Created > dateOfLastMonth);

            List<Expression<Func<Order, bool>>> orderFilters = new()
            {
                e => !e.IsDeleted,
            };
            var orders = await _unitOfWork.OrderRepository.GetAllAsync(orderFilters);
            var lastMonthOrders = orders.Count(e => e.Created > dateOfLastMonth);
            var increase = (lastMonthOrders / ((orders.Count - lastMonthOrders) > 0 ? (orders.Count - lastMonthOrders) : 1)).ToString("0.00%");
            var response = new ActiveOrdersReservations { Reservations = reservations.Count, Orders = orders.Count, Increase = increase };

            return await Task.FromResult(new Response<ActiveOrdersReservations>(response));
        }
    }
}
