using Application.Foods.Response;
using Application.Models;
using AutoMapper;
using Core.Common;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using System.Linq.Expressions;

namespace Application.Foods.Queries
{
    public class GetFoodWithPaginationQuery : PaginationRequest, IRequest<Response<PaginatedList<FoodDto>>>
    {
        public new FoodProperty? OrderBy { get; init; }
        public bool? Available { get; init; }
    }

    public class GetFoodWithPaginationQueryHandler : IRequestHandler<GetFoodWithPaginationQuery, Response<PaginatedList<FoodDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetFoodWithPaginationQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<PaginatedList<FoodDto>>> Handle(GetFoodWithPaginationQuery request, CancellationToken cancellationToken)
        {
            List<Expression<Func<Food, bool>>> filters = new();
            Func<IQueryable<Food>, IOrderedQueryable<Food>> orderBy = null;
            string includeProperties = "";

            if (!string.IsNullOrWhiteSpace(request.SearchValue))
            {
                filters.Add(e => e.Ingredient.Contains(request.SearchValue) || e.Name.Contains(request.SearchValue));
            }
            if (request.Available is not null)
            {
                filters.Add(e => e.Available == request.Available);
            }

            switch (request.OrderBy)
            {
                case (FoodProperty.Name):
                    if (request.IsDescending)
                    {
                        orderBy = e => e.OrderByDescending(x => x.Name);
                        break;
                    }
                    orderBy = e => e.OrderBy(x => x.Name);
                    break;
                case (FoodProperty.Ingredient):
                    if (request.IsDescending)
                    {
                        orderBy = e => e.OrderByDescending(x => x.Ingredient);
                        break;
                    }
                    orderBy = e => e.OrderBy(x => x.Ingredient);
                    break;
                case (FoodProperty.Available):
                    if (request.IsDescending)
                    {
                        orderBy = e => e.OrderByDescending(x => x.Available);
                        break;
                    }
                    orderBy = e => e.OrderBy(x => x.Available);
                    break;
                default:
                    break;
            }

            var result = await _unitOfWork.FoodRepository.GetPaginatedListAsync(filters, orderBy, includeProperties, request.PageIndex, request.PageSize);
            var mappedResult = _mapper.Map<PaginatedList<Food>, PaginatedList<FoodDto>>(result);
            return new Response<PaginatedList<FoodDto>>(mappedResult);
        }
    }
}
