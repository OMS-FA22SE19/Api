using Application.Common.Exceptions;
using Application.Foods.Response;
using Application.Models;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace Application.Foods.Queries
{
    public class GetFoodWithMenuIdAndCategoryIdQuery : IRequest<Response<List<FoodDto>>>
    {
        [Required]
        public int MenuId { get; init; }
        [Required]
        public int CategoryId { get; set; }
    }

    public sealed class GetFoodWithMenuIdAndCategoryIdQueryHandler : IRequestHandler<GetFoodWithMenuIdAndCategoryIdQuery, Response<List<FoodDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetFoodWithMenuIdAndCategoryIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<List<FoodDto>>> Handle(GetFoodWithMenuIdAndCategoryIdQuery request, CancellationToken cancellationToken)
        {
            var menu = await _unitOfWork.MenuRepository.GetAsync(e => e.Id == request.MenuId);
            if (menu is null)
            {
                throw new NotFoundException(nameof(Menu), request.MenuId);
            }

            var category = await _unitOfWork.CategoryRepository.GetAsync(e => e.Id == request.CategoryId);
            if (category is null)
            {
                throw new NotFoundException(nameof(Category), request.CategoryId);
            }

            List<Expression<Func<Food, bool>>> filters = new();
            Func<IQueryable<Food>, IOrderedQueryable<Food>> orderBy = null;
            string includeProperties = $"{nameof(Food.FoodCategories)}.{nameof(FoodCategory.Category)},{nameof(Food.MenuFoods)}";

            filters.Add(e => e.FoodCategories.Any(c => c.CategoryId == request.CategoryId) && e.MenuFoods.Any(m => m.MenuId == request.MenuId));

            var result = await _unitOfWork.FoodRepository.GetAllAsync(filters, orderBy, includeProperties);
            var mappedResult = _mapper.Map<List<FoodDto>>(result);

            return new Response<List<FoodDto>>(mappedResult);
        }
    }
}
