using Application.Categories.Response;
using Application.Foods.Response;
using Application.Models;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Foods.Queries
{
    public sealed class GetFoodWithIdQuery : IRequest<Response<FoodDto>>
    {
        [Required]
        public int Id { get; init; }
    }

    public sealed class GetFoodWithIdQueryHandler : IRequestHandler<GetFoodWithIdQuery, Response<FoodDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetFoodWithIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<FoodDto>> Handle(GetFoodWithIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _unitOfWork.FoodRepository.GetAsync(e => e.Id == request.Id, $"{nameof(Food.FoodCategories)}.{nameof(Category)}");
            var mappedResult = _mapper.Map<FoodDto>(result);
            if (mappedResult.Categories is null)
            {
                mappedResult.Categories = new List<CategoryDto>();
            }
            foreach (var category in result.FoodCategories.Select(e => e.Category))
            {
                if (category is not null)
                {
                    mappedResult.Categories.Add(_mapper.Map<CategoryDto>(category));
                }
            }
            return new Response<FoodDto>(mappedResult);
        }
    }
}
