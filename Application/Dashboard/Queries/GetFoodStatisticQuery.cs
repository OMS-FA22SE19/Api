using Application.Common.Interfaces;
using Application.Dashboard.Response;
using Application.Models;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.Linq.Expressions;

namespace Application.Dashboard.Queries
{
    public sealed class GetFoodStatisticQuery : IRequest<Response<FoodStatistic>>
    {

    }

    public sealed class GetFoodStatisticQueryHandler : IRequestHandler<GetFoodStatisticQuery, Response<FoodStatistic>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTime _dateTime;

        public GetFoodStatisticQueryHandler(IUnitOfWork unitOfWork, IDateTime dateTime)
        {
            _unitOfWork = unitOfWork;
            _dateTime = dateTime;
        }

        public async Task<Response<FoodStatistic>> Handle(GetFoodStatisticQuery request, CancellationToken cancellationToken)
        {
            List<Expression<Func<Food, bool>>> filters = new();
            filters.Add(e => !e.IsDeleted);
            var foods = await _unitOfWork.FoodRepository.GetAllAsync(filters);

            var dateOfLastMonth = new DateTime(_dateTime.Now.Year, _dateTime.Now.Month - 1, DateTime.DaysInMonth(_dateTime.Now.Year, _dateTime.Now.Month - 1));

            var lastMonthFoods = foods.Where(e => e.Created <= dateOfLastMonth).ToList();
            var increase = ((foods.Count - lastMonthFoods.Count) / (lastMonthFoods.Any() ? lastMonthFoods.Count : 1)).ToString("0.00%");
            var response = new FoodStatistic { Food = foods.Count, Increase = increase };

            return await Task.FromResult(new Response<FoodStatistic>(response));
        }
    }
}
