using Application.Foods.Events;
using Application.Foods.Response;
using Application.Models;
using AutoMapper;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Foods.Commands
{
    public sealed class DeleteFoodCommand : IRequest<Response<FoodDto>>
    {
        [Required]
        public int Id { get; init; }
    }

    public sealed class DeleteFoodCommandHandler : IRequestHandler<DeleteFoodCommand, Response<FoodDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public DeleteFoodCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<Response<FoodDto>> Handle(DeleteFoodCommand request, CancellationToken cancellationToken)
        {
            var result = await _unitOfWork.FoodRepository.DeleteAsync(e => e.Id == request.Id);
            await _mediator.Publish(new DeleteFoodEvent()
            {
                id = request.Id
            });
            await _unitOfWork.CompleteAsync(cancellationToken);
            return new Response<FoodDto>()
            {
                Succeeded = true,
                StatusCode = System.Net.HttpStatusCode.NoContent
            };
        }
    }
}

