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
        public int? numOfProcessing { get; set; }
        public int? numOfServed { get; set; }

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
            List<Expression<Func<OrderDetail, bool>>> filters = new();
            filters.Add(od => od.Status == OrderDetailStatus.Received && od.OrderId.Substring(0,5).Equals("demo-"));
            Func<IQueryable<OrderDetail>, IOrderedQueryable<OrderDetail>> orderBy = null;
            orderBy = e => e.OrderBy(x => x.CreatedBy);

            OrderReservationDemoDto dto = new OrderReservationDemoDto()
            {
                created = new List<string>(),
                updated = new List<string>(),
                Error = new List<string>()
            };

            var OrderDetailDemo = await _unitOfWork.OrderDetailRepository.GetAllAsync(filters, orderBy);
            for (int i = 0; i < request.numOfProcessing; i++)
            {
                if (i < OrderDetailDemo.Count)
                {
                    OrderDetailDemo[i].Status = OrderDetailStatus.Processing;
                    await _unitOfWork.OrderDetailRepository.UpdateAsync(OrderDetailDemo[i]);
                    await _unitOfWork.CompleteAsync(cancellationToken);
                }
                else
                {
                    dto.Error.Add($"Cannot update {request.numOfProcessing - i} because there not enough dishes");
                    break;
                }
            }
            for (int i = 0; i < request.numOfServed; i++)
            {
                int k = (int)(i + request.numOfProcessing + 1);
                if (k < OrderDetailDemo.Count)
                {
                    OrderDetailDemo[k].Status = OrderDetailStatus.Served;
                    await _unitOfWork.OrderDetailRepository.UpdateAsync(OrderDetailDemo[k]);
                    await _unitOfWork.CompleteAsync(cancellationToken);
                }
                else
                {
                    dto.Error.Add($"Cannot update {request.numOfServed - i} because there not enough dishes");
                    break;
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
