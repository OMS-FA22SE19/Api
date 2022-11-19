using Application.Common.Exceptions;
using Application.Foods.Response;
using Application.Common.Models;
using Application.Types.Response;
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
            var result = await _unitOfWork.FoodRepository.GetAsync(e => e.Id == request.Id, $"{nameof(Food.FoodTypes)}.{nameof(FoodType.Type)},{nameof(Food.CourseType)}");
            if (result is null)
            {
                throw new NotFoundException(nameof(Food), request.Id);
            }
            var mappedResult = _mapper.Map<FoodDto>(result);
            if (mappedResult.Types is null)
            {
                mappedResult.Types = new List<TypeDto>();
            }
            foreach (var foodType in result.FoodTypes.Select(e => e.Type))
            {
                if (foodType is not null)
                {
                    mappedResult.Types.Add(_mapper.Map<TypeDto>(foodType));
                }
            }
            return new Response<FoodDto>(mappedResult);
        }
    }
}
