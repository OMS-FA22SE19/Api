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
            List<Expression<Func<ApplicationUser, bool>>> filters = new();
            Func<IQueryable<ApplicationUser>, IOrderedQueryable<ApplicationUser>> orderBy = e => e.OrderByDescending(e => e.Orders.Count);
            string includeProperties = $"{nameof(ApplicationUser.Orders)}.{nameof(Order.OrderDetails)}";

            var dateOfFirstMonth = new DateTime(_dateTime.Now.Year, 1, 1);
            filters.Add(e => !e.IsDeleted);
            filters.Add(e => e.Orders.Any(e => e.Date >= dateOfFirstMonth && e.Status == OrderStatus.Paid));

            var customers = await _unitOfWork.UserRepository.GetPaginatedListAsync(filters, orderBy, includeProperties, 1, 10);
            var result = customers.Select(e => new RoyalCustomer
            {
                Name = e.FullName,
                PhoneNumber = e.PhoneNumber,
                Quantity = e.Orders.Count,
                Cost = e.Orders.Sum(e => e.OrderDetails.Sum(e => e.Price) - e.PrePaid)
            }).ToList();

            return new Response<List<RoyalCustomer>>(result);
        }
    }
}
