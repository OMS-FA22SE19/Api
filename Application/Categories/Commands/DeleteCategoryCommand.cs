using Application.Categories.Response;
using Application.Models;
using AutoMapper;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Categories.Commands
{
    public sealed class DeleteCategoryCommand : IRequest<Response<CategoryDto>>
    {
        [Required]
        public int Id { get; init; }
    }

    public sealed class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Response<CategoryDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DeleteCategoryCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<CategoryDto>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            var result = await _unitOfWork.CategoryRepository.DeleteAsync(e => e.Id == request.Id);
            await _unitOfWork.CompleteAsync(cancellationToken);
            return new Response<CategoryDto>()
            {
                Succeeded = true,
                StatusCode = System.Net.HttpStatusCode.NoContent
            };
        }
    }
}
