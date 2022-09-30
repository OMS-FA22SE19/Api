using Application.Common.Exceptions;
using Application.Foods.Response;
using Application.Models;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using Type = Core.Entities.Type;

namespace Application.Foods.Queries
{
    public class GetFoodWithMenuIdAndTypeIdQuery : IRequest<Response<List<FoodDto>>>
    {
        [Required]
        public int MenuId { get; init; }
        [Required]
        public int TypeId { get; set; }
    }

    public sealed class GetFoodWithMenuIdAndTypeIdQueryHandler : IRequestHandler<GetFoodWithMenuIdAndTypeIdQuery, Response<List<FoodDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetFoodWithMenuIdAndTypeIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<List<FoodDto>>> Handle(GetFoodWithMenuIdAndTypeIdQuery request, CancellationToken cancellationToken)
        {
            var menu = await _unitOfWork.MenuRepository.GetAsync(e => e.Id == request.MenuId);
            if (menu is null)
            {
                throw new NotFoundException(nameof(Menu), request.MenuId);
            }

            var category = await _unitOfWork.TypeRepository.GetAsync(e => e.Id == request.TypeId);
            if (category is null)
            {
                throw new NotFoundException(nameof(Type), request.TypeId);
            }

            List<Expression<Func<Food, bool>>> filters = new();
            Func<IQueryable<Food>, IOrderedQueryable<Food>> orderBy = null;
            string includeProperties = $"{nameof(Food.FoodTypes)}.{nameof(FoodType.Type)},{nameof(Food.MenuFoods)},{nameof(Food.CourseType)}";

            filters.Add(e => e.FoodTypes.Any(c => c.TypeId == request.TypeId) && e.MenuFoods.Any(m => m.MenuId == request.MenuId));

            var result = await _unitOfWork.FoodRepository.GetAllAsync(filters, orderBy, includeProperties);
            var mappedResult = _mapper.Map<List<FoodDto>>(result);

            return new Response<List<FoodDto>>(mappedResult);
        }
    }
}
