using Application.Common.Exceptions;
using Application.Models;
using Application.Orders.Response;
using AutoMapper;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;

namespace Application.Orders.Commands
{
    public sealed class CheckingBillCommand : IRequest<Response<OrderDto>>
    {
        public string Id { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<CheckingBillCommand, Order>();
        }
    }

    public sealed class CheckingBillCommandHandler : IRequestHandler<CheckingBillCommand, Response<OrderDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CheckingBillCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<OrderDto>> Handle(CheckingBillCommand request, CancellationToken cancellationToken)
        {
            var entity = await _unitOfWork.OrderRepository.GetAsync(e => e.Id == request.Id, $"{nameof(Order.OrderDetails)},{nameof(Order.User)}");
            if (entity is null)
            {
                throw new NotFoundException(nameof(Order), request.Id);
            }

            MapToEntity(entity);
            var result = await _unitOfWork.OrderRepository.UpdateAsync(entity);
            await _unitOfWork.CompleteAsync(cancellationToken);

            if (result is null)
            {
                return new Response<OrderDto>("error");
            }

            return new Response<OrderDto>()
            {
                StatusCode = System.Net.HttpStatusCode.NoContent
            };
        }

        private void MapToEntity(Order entity)
        {
            entity.Status = OrderStatus.Checking;
        }
    }
}
