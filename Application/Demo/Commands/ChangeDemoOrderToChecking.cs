using Application.Common.Exceptions;
using Application.Common.Mappings;
using Application.Demo.Responses;
using Application.Models;
using Application.OrderDetails.Events;
using Application.OrderDetails.Response;
using Application.Orders.Response;
using Application.Tables.Response;
using AutoMapper;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace Application.Demo.Commands
{
    public sealed class ChangeDemoOrderToChecking : IRequest<Response<OrderReservationDemoDto>>
    {
        public List<string> OrderIdsToChecking { get; set; }
    }

    public sealed class ChangeDemoOrderToCheckingHandler : IRequestHandler<ChangeDemoOrderToChecking, Response<OrderReservationDemoDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ChangeDemoOrderToCheckingHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<OrderReservationDemoDto>> Handle(ChangeDemoOrderToChecking request, CancellationToken cancellationToken)
        {
            OrderReservationDemoDto dto = new OrderReservationDemoDto()
            {
                created = new List<string>(),
                updated = new List<string>(),
                Error = new List<string>()
            };

            foreach (var id in request.OrderIdsToChecking)
            {
                var OrderDemoChecking = await _unitOfWork.OrderRepository.GetAsync(od => od.Id.Equals(id));
                if (OrderDemoChecking is null)
                {
                    dto.Error.Add($"Cannot update {id} because it didnt exist");
                }
                else
                {
                    OrderDemoChecking.Status = OrderStatus.Checking;
                    await _unitOfWork.OrderRepository.UpdateAsync(OrderDemoChecking);
                    await _unitOfWork.CompleteAsync(cancellationToken);
                    dto.updated.Add(id);
                }
            }
            return new Response<OrderReservationDemoDto>(dto)
            {
                StatusCode = System.Net.HttpStatusCode.Created
            };
        }

        //private void MapToEntity(ChangeDemoOrderToChecking request, OrderDetail entity) => entity.Status = request.Status;
    }
}
