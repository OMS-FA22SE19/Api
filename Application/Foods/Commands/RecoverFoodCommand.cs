using Application.Common.Exceptions;
using Application.Foods.Response;
using Application.Models;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Foods.Commands
{
    public sealed class RecoverFoodCommand : IRequest<Response<FoodDto>>
    {
        [Required]
        public int Id { get; init; }
    }

    public sealed class RecoverFoodCommandHandler : IRequestHandler<RecoverFoodCommand, Response<FoodDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RecoverFoodCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<FoodDto>> Handle(RecoverFoodCommand request, CancellationToken cancellationToken)
        {
            var deletedEntity = await _unitOfWork.FoodRepository.GetAsync(e => e.Id == request.Id && e.IsDeleted);
            if (deletedEntity is null)
            {
                throw new NotFoundException(nameof(Food), request.Id);
            }
            await _unitOfWork.FoodRepository.RestoreAsync(deletedEntity);
            await _unitOfWork.CompleteAsync(cancellationToken);
            return new Response<FoodDto>()
            {
                Succeeded = true,
                StatusCode = System.Net.HttpStatusCode.NoContent
            };
        }
    }
}
