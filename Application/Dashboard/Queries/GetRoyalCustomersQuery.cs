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
    public sealed class GetRoyalCustomersQuery : IRequest<Response<List<RoyalCustomer>>>
    {
    }

    public sealed class GetRoyalCustomersQueryHandler : IRequestHandler<GetRoyalCustomersQuery, Response<List<RoyalCustomer>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTime _dateTime;

        public GetRoyalCustomersQueryHandler(IUnitOfWork unitOfWork, IDateTime dateTime)
        {
            _unitOfWork = unitOfWork;
            _dateTime = dateTime;
        }

        public async Task<Response<List<RoyalCustomer>>> Handle(GetRoyalCustomersQuery request, CancellationToken cancellationToken)
        {
            List<Expression<Func<Reservation, bool>>> filters = new();
            Func<IQueryable<Reservation>, IOrderedQueryable<Reservation>> orderBy = null;
            string includeProperties = $"{nameof(Reservation.User)},{nameof(Reservation.Billing)}";

            var dateOfFirstMonth = new DateTime(_dateTime.Now.Year, 1, 1);
            filters.Add(e => !e.IsDeleted);
            filters.Add(e => e.StartTime >= dateOfFirstMonth && e.Status == ReservationStatus.Done);
            var reservations = await _unitOfWork.ReservationRepository.GetAllAsync(filters, orderBy, includeProperties);

            var result = new List<RoyalCustomer>();

            foreach (var reservation in reservations)
            {
                var user = result.FirstOrDefault(e => e.PhoneNumber.Equals(reservation.PhoneNumber));
                if (user is null)
                {
                    result.Add(new RoyalCustomer
                    {
                        Name = reservation.FullName,
                        PhoneNumber = reservation.PhoneNumber,
                        Cost = reservation.Billing.ReservationAmount + reservation.Billing.OrderAmount,
                        Quantity = 1
                    });
                }
                else
                {
                    user.Cost += reservation.Billing.ReservationAmount + reservation.Billing.OrderAmount;
                    user.Quantity += 1;
                }
            }

            return new Response<List<RoyalCustomer>>(result.OrderBy(e => e.Cost).ToList());
        }
    }
}
