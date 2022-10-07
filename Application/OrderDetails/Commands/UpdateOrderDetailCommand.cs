using Application.Common.Exceptions;
using Application.Common.Mappings;
using Application.Models;
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

        private static void MapToEntity(UpdateOrderDetailCommand request, OrderDetail entity) => entity.Status = request.Status;
    }
}
