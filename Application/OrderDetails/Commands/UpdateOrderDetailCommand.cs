using Application.Common.Exceptions;
using Application.Common.Mappings;
using Application.Models;
using Application.OrderDetails.Events;
using Application.OrderDetails.Response;
using AutoMapper;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.OrderDetails.Commands
{
    public sealed class UpdateOrderDetailCommand : IMapFrom<OrderDetail>, IRequest<Response<DishDto>>
    {
        [Required]
        public int Id { get; set; }
        public OrderDetailStatus Status { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<UpdateOrderDetailCommand, OrderDetail>();
        }
    }

    public sealed class UpdateOrderDetailCommandHandler : IRequestHandler<UpdateOrderDetailCommand, Response<DishDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateOrderDetailCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<DishDto>> Handle(UpdateOrderDetailCommand request, CancellationToken cancellationToken)
        {
            var entity = await _unitOfWork.OrderDetailRepository.GetAsync(e => e.Id == request.Id);
            if (entity is null)
            {
                throw new NotFoundException(nameof(OrderDetail), request.Id);
            }
            switch (request.Status)
            {
                case OrderDetailStatus.Cancelled:
                    if (entity.Status != OrderDetailStatus.Received)
                    {
                        return new Response<DishDto>("Invalid Operation! You can only cancel received dish")
                        {
                            StatusCode = System.Net.HttpStatusCode.BadRequest
                        };
                    }
                    break;
                case OrderDetailStatus.Processing:
                    if (entity.Status != OrderDetailStatus.Received)
                    {
                        return new Response<DishDto>("Invalid Operation! You can only procceed received dish")
                        {
                            StatusCode = System.Net.HttpStatusCode.BadRequest
                        };
                    }
                    break;
                case OrderDetailStatus.Served:
                    if (entity.Status != OrderDetailStatus.Processing)
                    {
                        return new Response<DishDto>("Invalid Operation! You can only serve processing dish")
                        {
                            StatusCode = System.Net.HttpStatusCode.BadRequest
                        };
                    }
                    break;
                default:
                    break;
            }
            MapToEntity(request, entity);

            var result = await _unitOfWork.OrderDetailRepository.UpdateAsync(entity);
            var food = await _unitOfWork.FoodRepository.GetAsync(f => f.Id == entity.FoodId);
            var order = await _unitOfWork.OrderRepository.GetAsync(o => o.Id.Equals(entity.OrderId));
            var token = await _unitOfWork.UserDeviceTokenRepository.GetAsync(t => t.userId.Equals(order.UserId));
            if (token is not null)
            {
                entity.AddDomainEvent(new UpdateOrderDetailEvent
                {
                    Id = request.Id,
                    name = food.Name,
                    Status = request.Status,
                    token = token.deviceToken
                });
            } 
            else
            {
                entity.AddDomainEvent(new UpdateOrderDetailEvent
                {
                    Id = request.Id,
                    name = food.Name,
                    Status = request.Status,
                    token = ""
                });
            }
            
            await _unitOfWork.CompleteAsync(cancellationToken);
            if (result is null)
            {
                return new Response<DishDto>("error");
            }
            var mappedResult = _mapper.Map<DishDto>(result);
            return new Response<DishDto>()
            {
                Succeeded = true,
                StatusCode = System.Net.HttpStatusCode.NoContent
            };
        }

        private void MapToEntity(UpdateOrderDetailCommand request, OrderDetail entity) => entity.Status = request.Status;
    }
}
