using Application.Common.Exceptions;
using Application.Foods.Response;
using Application.Models;
using Application.Types.Response;
using AutoMapper;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace Application.Foods.Queries
{
    public class GetFoodWithMenuIdQuery : PaginationRequest, IRequest<Response<List<FoodDto>>>
    {
        [Required]
        public int MenuId { get; init; }
        public int? CourseTypeId { get; init; }
        public int? TypeId { get; init; }
        public new FoodProperty? OrderBy { get; init; }
    }

    public sealed class GetFoodWithMenuIdQueryHandler : IRequestHandler<GetFoodWithMenuIdQuery, Response<List<FoodDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetFoodWithMenuIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<List<FoodDto>>> Handle(GetFoodWithMenuIdQuery request, CancellationToken cancellationToken)
        {
            var menu = await _unitOfWork.MenuRepository.GetAsync(e => e.Id == request.MenuId);
            if (menu is null)
            {
                throw new NotFoundException(nameof(Menu), request.MenuId);
            }

            List<Expression<Func<Food, bool>>> filters = new();
            Func<IQueryable<Food>, IOrderedQueryable<Food>> orderBy = null;
            string includeProperties = $"{nameof(Food.FoodTypes)}.{nameof(FoodType.Type)},{nameof(Food.MenuFoods)},{nameof(Food.CourseType)}";

            filters.Add(e => e.MenuFoods.Any(m => m.MenuId == request.MenuId));

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
            var mappedResult = _mapper.Map<List<FoodDto>>(result);
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
            return new Response<List<FoodDto>>(mappedResult);
        }
    }
}
