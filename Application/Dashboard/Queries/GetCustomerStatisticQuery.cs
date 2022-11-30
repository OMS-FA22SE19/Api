using Application.Common.Interfaces;
using Application.Dashboard.Response;
using Application.Models;
using Core.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Dashboard.Queries
{
    public sealed class GetCustomerStatisticQuery : IRequest<Response<CustomerStatistic>>
    {

    }

    public sealed class GetCustomerStatisticQueryHandler : IRequestHandler<GetCustomerStatisticQuery, Response<CustomerStatistic>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDateTime _dateTime;

        public GetCustomerStatisticQueryHandler(UserManager<ApplicationUser> userManager, IDateTime dateTime)
        {
            _userManager = userManager;
            _dateTime = dateTime;
        }

        public async Task<Response<CustomerStatistic>> Handle(GetCustomerStatisticQuery request, CancellationToken cancellationToken)
        {
            var customerRole = "Customer";
            var customer = (await _userManager.GetUsersInRoleAsync(customerRole)).Where(e => !e.IsDeleted).ToList();
            var dateOfLastMonth = new DateTime(_dateTime.Now.Year, _dateTime.Now.Month - 1, DateTime.DaysInMonth(_dateTime.Now.Year, _dateTime.Now.Month - 1));
            var lastMonthCustomers = customer.Where(e => e.Created <= dateOfLastMonth).ToList();
            var increase = ((customer.Count - lastMonthCustomers.Count) / (lastMonthCustomers.Any() ? lastMonthCustomers.Count : 1)).ToString("0.00%");
            var response = new CustomerStatistic { Customers = customer.Count, Increase = increase };

            return await Task.FromResult(new Response<CustomerStatistic>(response));
        }
    }
}
