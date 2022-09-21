using Application.Categories.Response;
using Application.Common.Exceptions;
using Application.Common.Mappings;
using Application.Models;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Categories.Commands
{
    public sealed class UpdateCategoryCommand : IMapFrom<Category>, IRequest<Response<CategoryDto>>
    {
        [Required]
        public int Id { get; init; }
        [Required]
        [StringLength(1000, MinimumLength = 5)]
        public string Name { get; init; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<UpdateCategoryCommand, Category>();
        }
    }

    public sealed class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Response<CategoryDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateCategoryCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<CategoryDto>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            var entity = await _unitOfWork.CategoryRepository.GetAsync(e => e.Id == request.Id);
            if (entity is null)
            {
                throw new NotFoundException(nameof(Category), request.Id);
            }
            var updatedEntity = _mapper.Map<Category>(request);
            var result = await _unitOfWork.CategoryRepository.UpdateAsync(updatedEntity);
            await _unitOfWork.CompleteAsync(cancellationToken);
            if (result is null)
            {
                return new Response<CategoryDto>("error");
            }
            var mappedResult = _mapper.Map<CategoryDto>(result);
            return new Response<CategoryDto>()
            {
                Succeeded = true,
                StatusCode = System.Net.HttpStatusCode.NoContent
            };
        }
    }
}
