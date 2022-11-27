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
    public sealed class ChangeOrderDetailStatusDemo : IMapFrom<OrderDetail>, IRequest<Response<OrderReservationDemoDto>>
    {
        public List<string>? OrderIdsToProcessing { get; set; }
        public List<string>? OrderIdsToServed { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ChangeOrderDetailStatusDemo, OrderDetail>();
        }
    }

    public sealed class ChangeOrderDetailStatusDemoHandler : IRequestHandler<ChangeOrderDetailStatusDemo, Response<OrderReservationDemoDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ChangeOrderDetailStatusDemoHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<OrderReservationDemoDto>> Handle(ChangeOrderDetailStatusDemo request, CancellationToken cancellationToken)
        {

            OrderReservationDemoDto dto = new OrderReservationDemoDto()
            {
                created = new List<string>(),
                updated = new List<string>(),
                Error = new List<string>()
            };
            
            foreach (var id in request.OrderIdsToProcessing)
            {
                List<Expression<Func<OrderDetail, bool>>> filters = new();
                filters.Add(od => od.Status != OrderDetailStatus.Cancelled && od.OrderId.Equals(id));
                var OrderDetailDemoForProcessing = await _unitOfWork.OrderDetailRepository.GetAllAsync(filters);
                if (OrderDetailDemoForProcessing is null) 
                {
                    dto.Error.Add($"Cannot update {id} because there not enough dishes");
                }
                else
                {
                    foreach(var detail in OrderDetailDemoForProcessing)
                    {
                        detail.Status = OrderDetailStatus.Processing;
                        await _unitOfWork.OrderDetailRepository.UpdateAsync(detail);
                        await _unitOfWork.CompleteAsync(cancellationToken);
                        dto.updated.Add(id);
                    }
                }
            }
            foreach (var id in request.OrderIdsToServed)
            {
                List<Expression<Func<OrderDetail, bool>>> filters = new();
                filters.Add(od => od.Status != OrderDetailStatus.Cancelled && od.OrderId.Equals(id));
                var OrderDetailDemoForCancelled = await _unitOfWork.OrderDetailRepository.GetAllAsync(filters);
                if (OrderDetailDemoForCancelled is null)
                {
                    dto.Error.Add($"Cannot update {id} because there not enough dishes");
                }
                else
                {
                    foreach (var detail in OrderDetailDemoForCancelled)
                    {
                        detail.Status = OrderDetailStatus.Served;
                        await _unitOfWork.OrderDetailRepository.UpdateAsync(detail);
                        await _unitOfWork.CompleteAsync(cancellationToken);
                        dto.updated.Add(id);
                    }
                }
            }
            return new Response<OrderReservationDemoDto>(dto)
            {
                StatusCode = System.Net.HttpStatusCode.Created
            };
        }

        //private void MapToEntity(ChangeOrderDetailStatusDemo request, OrderDetail entity) => entity.Status = request.Status;
    }
}
