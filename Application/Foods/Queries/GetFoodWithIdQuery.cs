using Application.Foods.Response;
using Application.Models;
using AutoMapper;
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
            var result = await _unitOfWork.FoodRepository.GetAsync(e => e.Id == request.Id);
            var mappedResult = _mapper.Map<FoodDto>(result);
            return new Response<FoodDto>(mappedResult);
        }
    }
}
