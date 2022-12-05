using Application.Foods.Response;
using Application.Models;
using Application.Types.Response;
using AutoMapper;
using Core.Common;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using System.Linq.Expressions;
using Type = Core.Entities.Type;

namespace Application.Foods.Queries
{
    public sealed class GetFoodWithPaginationQuery : PaginationRequest, IRequest<Response<PaginatedList<FoodDto>>>
    {
        public FoodProperty SearchBy { get; init; }
        public FoodProperty? OrderBy { get; init; }
        public bool? Available { get; init; }
    }

    public sealed class GetFoodWithPaginationQueryHandler : IRequestHandler<GetFoodWithPaginationQuery, Response<PaginatedList<FoodDto>>>
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
            string includeProperties = $"{nameof(Food.FoodTypes)}.{nameof(Type)},{nameof(Food.CourseType)}";

            if (!string.IsNullOrWhiteSpace(request.SearchValue))
            {
                switch (request.SearchBy)
                {
                    case FoodProperty.Name:
                        filters.Add(e => e.Name.Contains(request.SearchValue));
                        break;
                    case FoodProperty.Description:
                        filters.Add(e => e.Description.Contains(request.SearchValue));
                        break;
                    case FoodProperty.Ingredient:
                        filters.Add(e => e.Ingredient.Contains(request.SearchValue));
                        break;
                    case FoodProperty.Available:
                        break;
                    case FoodProperty.CourseType:
                        List<Expression<Func<CourseType, bool>>> courseTypeFilters = new();
                        courseTypeFilters.Add(e => e.Name.Contains(request.SearchValue) && !e.IsDeleted);
                        var courseTypes = await _unitOfWork.CourseTypeRepository.GetAllAsync(courseTypeFilters, null, null);
                        var courseTypeIds = courseTypes.Select(e => e.Id).ToList();
                        filters.Add(e => courseTypeIds.Contains(e.CourseTypeId));
                        break;
                    default:
                        break;
                }
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
                case (FoodProperty.Description):
                    if (request.IsDescending)
                    {
                        orderBy = e => e.OrderByDescending(x => x.Description);
                        break;
                    }
                    orderBy = e => e.OrderBy(x => x.Description);
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
            if (!string.IsNullOrWhiteSpace(request.SearchValue) && request.SearchBy == FoodProperty.FoodType)
            {
                List<Expression<Func<Type, bool>>> typeFilters = new();
                typeFilters.Add(e => e.Name.Contains(request.SearchValue) && !e.IsDeleted);
                var types = await _unitOfWork.TypeRepository.GetAllAsync(typeFilters, null, null);
                var typeIds = types.Select(e => e.Id).ToList();
                var items = result.Where(e => e.FoodTypes.Any(x => typeIds.Contains(x.TypeId))).ToList();
                result = new PaginatedList<Food>(pageNumber: result.PageNumber, totalPages: result.TotalPages, totalCount: result.TotalCount, items: items);
            }
            var mappedResult = _mapper.Map<PaginatedList<Food>, PaginatedList<FoodDto>>(result);
            foreach (var item in mappedResult)
            {
                if (item.Types is null)
                {
                    item.Types = new List<TypeDto>();
                }
                foreach (var foodType in result.FirstOrDefault(e => e.Id == item.Id)?.FoodTypes?.Select(e => e.Type))
                {
                    if (foodType is not null)
                    {
                        item.Types.Add(_mapper.Map<TypeDto>(foodType));

                    }
                }
            }
            return new Response<PaginatedList<FoodDto>>(mappedResult);
        }
    }
}
