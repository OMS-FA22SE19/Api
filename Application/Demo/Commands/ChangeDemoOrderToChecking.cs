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
        public int? numOfOrder { get; set; }
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
            List<Expression<Func<Order, bool>>> filters = new();
            filters.Add(od => od.Status == OrderStatus.Processing && od.Id.Substring(0, 5).Equals("demo-"));
            Func<IQueryable<Order>, IOrderedQueryable<Order>> orderBy = null;
            orderBy = e => e.OrderBy(x => x.CreatedBy);

            OrderReservationDemoDto dto = new OrderReservationDemoDto()
            {
                created = new List<string>(),
                updated = new List<string>(),
                Error = ""
            };

            var orders = await _unitOfWork.OrderRepository.GetAllAsync(filters, orderBy);
            for (int i = 0; i < request.numOfOrder; i++)
            {
                if (i < orders.Count)
                {
                    orders[i].Status = OrderStatus.Checking;
                    await _unitOfWork.OrderRepository.UpdateAsync(orders[i]);
                    await _unitOfWork.CompleteAsync(cancellationToken);
                    dto.updated.Add(orders[i].Id);
                }
                else
                {
                    dto.Error = $"Cannot update {request.numOfOrder - i} because there not enough order";
                    break;
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
