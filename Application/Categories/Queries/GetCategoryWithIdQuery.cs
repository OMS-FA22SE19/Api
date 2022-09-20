using Application.Categories.Response;
using Application.Models;
using AutoMapper;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Categories.Queries
{
    public sealed class GetCategoryWithIdQuery : IRequest<Response<CategoryDto>>
    {
        [Required]
        public int Id { get; init; }
    }

    public sealed class GetCategoryWithIdQueryHandler : IRequestHandler<GetCategoryWithIdQuery, Response<CategoryDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetCategoryWithIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<CategoryDto>> Handle(GetCategoryWithIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _unitOfWork.CategoryRepository.GetAsync(e => e.Id == request.Id);
            var mappedResult = _mapper.Map<CategoryDto>(result);
            return new Response<CategoryDto>(mappedResult);
        }
    }
}
