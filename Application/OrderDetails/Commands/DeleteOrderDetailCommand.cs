using Application.Models;
using Application.OrderDetails.Response;
using AutoMapper;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.OrderDetails.Commands
{
    public sealed class DeleteOrderDetailCommand : IRequest<Response<OrderDetailDto>>
    {
        [Required]
        public int Id { get; init; }
    }

    public sealed class DeleteOrderDetailCommandHandler : IRequestHandler<DeleteOrderDetailCommand, Response<OrderDetailDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DeleteOrderDetailCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<OrderDetailDto>> Handle(DeleteOrderDetailCommand request, CancellationToken cancellationToken)
        {
            var entity = await _unitOfWork.OrderDetailRepository.GetAsync(e => e.Id == request.Id);
            if (entity.Status != Core.Enums.OrderDetailStatus.Received)
            {
                return new Response<OrderDetailDto>("Invalid Operation! You can only remove received dish")
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                };
            }
            var result = await _unitOfWork.OrderDetailRepository.DeleteAsync(e => e.Id == request.Id);
            await _unitOfWork.CompleteAsync(cancellationToken);
            return new Response<OrderDetailDto>()
            {
                Succeeded = true,
                StatusCode = System.Net.HttpStatusCode.NoContent
            };
        }
    }
}

