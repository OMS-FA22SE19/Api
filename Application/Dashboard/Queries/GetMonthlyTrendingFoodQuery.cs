using Application.Common.Interfaces;
using Application.Dashboard.Response;
using Application.Models;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.Linq.Expressions;

namespace Application.Dashboard.Queries
{
    public sealed class GetMonthlyTrendingFoodQuery : IRequest<Response<List<MonthlyTrendingFood>>>
    {
    }

    public sealed class GetMonthlyTrendingFoodQueryHandler : IRequestHandler<GetMonthlyTrendingFoodQuery, Response<List<MonthlyTrendingFood>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTime _dateTime;

        public GetMonthlyTrendingFoodQueryHandler(IUnitOfWork unitOfWork, IDateTime dateTime)
        {
            _unitOfWork = unitOfWork;
            _dateTime = dateTime;
        }

        public async Task<Response<List<MonthlyTrendingFood>>> Handle(GetMonthlyTrendingFoodQuery request, CancellationToken cancellationToken)
        {
            List<Expression<Func<Food, bool>>> filters = new();
            Func<IQueryable<Food>, IOrderedQueryable<Food>> orderBy = e => e.OrderByDescending(e => e.OrderDetails.Count);
            string includeProperties = $"{nameof(Food.OrderDetails)}.{nameof(OrderDetail.Order)}";

            var dateOfLastMonth = new DateTime(_dateTime.Now.Year, _dateTime.Now.Month - 1, 1);
            filters.Add(e => !e.IsDeleted);
            filters.Add(e => e.OrderDetails.Any(o => o.Order.Date >= dateOfLastMonth));

            var food = await _unitOfWork.FoodRepository.GetPaginatedListAsync(filters, orderBy, includeProperties, 1, 10);
            var result = food.Select(e => new MonthlyTrendingFood
            {
                Name = e.Name,
                PictureUrl = e.PictureUrl,
                Quantity = e.OrderDetails.Count
            }).ToList();

            return new Response<List<MonthlyTrendingFood>>(result.OrderByDescending(e => e.Quantity).ToList());
        }
    }
}
