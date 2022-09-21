using Application.Categories.Response;
using Application.Common.Mappings;
using Application.Models;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Categories.Commands
{
    public sealed class CreateCategoryCommand : IMapFrom<Category>, IRequest<Response<CategoryDto>>
    {
        [Required]
        [StringLength(1000, MinimumLength = 5)]
        public string Name { get; init; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<CreateCategoryCommand, Category>();
        }
    }

    public sealed class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Response<CategoryDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateCategoryCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Category>(request);
            var result = await _unitOfWork.CategoryRepository.InsertAsync(entity);
            await _unitOfWork.CompleteAsync(cancellationToken);
            if (result is null)
            {
                return new Response<CategoryDto>("error");
            }
            var mappedResult = _mapper.Map<CategoryDto>(result);
            return new Response<CategoryDto>(mappedResult)
            {
                StatusCode = System.Net.HttpStatusCode.Created
            };
        }
    }
}
