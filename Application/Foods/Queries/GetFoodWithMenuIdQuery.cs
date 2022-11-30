using Application.Common.Exceptions;
using Application.Foods.Response;
using Application.Models;
using Application.Types.Response;
using AutoMapper;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using Type = Core.Entities.Type;

namespace Application.Foods.Queries
{
    public class GetFoodWithMenuIdQuery : PaginationRequest, IRequest<Response<List<MenuFoodDto>>>
    {
        [NotMapped]
        public int MenuId { get; set; }
        public int? CourseTypeId { get; set; }
        public int? TypeId { get; set; }
        public FoodProperty SearchBy { get; set; }
        public string? SearchValue { get; set; }
        public FoodProperty? OrderBy { get; set; }
    }

    public sealed class GetFoodWithMenuIdQueryHandler : IRequestHandler<GetFoodWithMenuIdQuery, Response<List<MenuFoodDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetFoodWithMenuIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<List<MenuFoodDto>>> Handle(GetFoodWithMenuIdQuery request, CancellationToken cancellationToken)
        {
            var menu = await _unitOfWork.MenuRepository.GetAsync(e => e.Id == request.MenuId && !e.IsDeleted);
            if (menu is null)
            {
                throw new NotFoundException(nameof(Menu), request.MenuId);
            }

            List<Expression<Func<Food, bool>>> filters = new();
            Func<IQueryable<Food>, IOrderedQueryable<Food>> orderBy = null;
            string includeProperties = $"{nameof(Food.FoodTypes)}.{nameof(FoodType.Type)},{nameof(Food.MenuFoods)},{nameof(Food.CourseType)}";

            filters.Add(e => e.MenuFoods.Any(m => m.MenuId == request.MenuId));

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

            if (request.CourseTypeId is not null)
            {
                filters.Add(e => e.CourseTypeId == request.CourseTypeId);
            }
            if (request.TypeId is not null)
            {
                filters.Add(e => e.FoodTypes.Any(x => x.TypeId == request.TypeId));
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
            var result = await _unitOfWork.FoodRepository.GetAllAsync(filters, orderBy, includeProperties);
            if (!string.IsNullOrWhiteSpace(request.SearchValue) && request.SearchBy == FoodProperty.FoodType)
            {
                List<Expression<Func<Type, bool>>> typeFilters = new();
                typeFilters.Add(e => e.Name.Contains(request.SearchValue) && !e.IsDeleted);
                var types = await _unitOfWork.TypeRepository.GetAllAsync(typeFilters, null, null);
                var typeIds = types.Select(e => e.Id).ToList();
                result = result.Where(e => e.FoodTypes.Any(x => typeIds.Contains(x.TypeId))).ToList();
            }
            var mappedResult = _mapper.Map<List<MenuFoodDto>>(result);
            foreach (var item in mappedResult)
            {
                item.Price = result.First(e => e.Id == item.Id).MenuFoods.First(e => e.MenuId == request.MenuId).Price;
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
            return new Response<List<MenuFoodDto>>(mappedResult);
        }
    }
}
