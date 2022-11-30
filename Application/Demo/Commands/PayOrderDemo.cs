using Application.Common.Exceptions;
using Application.Common.Mappings;
using Application.Common.Models;
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Threading;

namespace Application.Demo.Commands
{
    public sealed class PayOrderDemo : IRequest<Response<OrderReservationDemoDto>>
    {
        public List<string> OrderIdsToPay { get; set; }
    }

    public sealed class PayOrderDemoHandler : IRequestHandler<PayOrderDemo, Response<OrderReservationDemoDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PayOrderDemoHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<OrderReservationDemoDto>> Handle(PayOrderDemo request, CancellationToken cancellationToken)
        {
            OrderReservationDemoDto dto = new OrderReservationDemoDto()
            {
                created = new List<string>(),
                updated = new List<string>(),
                Error = new List<string>()
            };

            foreach (var id in request.OrderIdsToPay)
            {
                var OrderDemoPaying = await _unitOfWork.OrderRepository.GetAsync(od => od.Id.Equals(id), $"{nameof(Order.OrderDetails)}");
                if (OrderDemoPaying is null)
                {
                    dto.Error.Add($"Cannot update {id} because it didnt exist");
                }
                else
                {
                    OrderDemoPaying.Status = OrderStatus.Checking;
                    await _unitOfWork.OrderRepository.UpdateAsync(OrderDemoPaying);

                    double total = 0;
                    foreach (var detail in OrderDemoPaying.OrderDetails)
                    {
                        if (detail.Status != OrderDetailStatus.Cancelled && detail.Status != OrderDetailStatus.Served)
                        {
                            detail.Status = OrderDetailStatus.Overcharged;
                            await _unitOfWork.OrderDetailRepository.UpdateAsync(detail);
                        }
                        total += detail.Price;
                    }

                    var billing = await _unitOfWork.BillingRepository.GetAsync(b => b.ReservationId == OrderDemoPaying.ReservationId);
                    var result = new Billing();
                    if (billing is null)
                    {
                        Billing bill = new Billing
                        {
                            Id = "demo-" + OrderDemoPaying.Id,
                            ReservationId = OrderDemoPaying.ReservationId,
                            OrderId = OrderDemoPaying.Id,
                            OrderAmount = total
                        };
                        result = await _unitOfWork.BillingRepository.InsertAsync(bill);
                    }
                    else
                    {
                        billing.OrderAmount = total - billing.ReservationAmount;
                        result = await _unitOfWork.BillingRepository.UpdateAsync(billing);
                    }

                    await _unitOfWork.CompleteAsync(cancellationToken);
                    dto.updated.Add(id);
                }
            }
            return new Response<OrderReservationDemoDto>(dto)
            {
                StatusCode = System.Net.HttpStatusCode.Created
            };
        }

        //private void MapToEntity(PayOrderDemo request, OrderDetail entity) => entity.Status = request.Status;
    }
}

//orders[i].Status = OrderStatus.Paid;
//await _unitOfWork.OrderRepository.UpdateAsync(orders[i]);

//double total = 0;
//foreach (var detail in orders[i].OrderDetails)
//{
//    if (detail.Status != OrderDetailStatus.Cancelled && detail.Status != OrderDetailStatus.Served)
//    {
//        detail.Status = OrderDetailStatus.Overcharged;
//        await _unitOfWork.OrderDetailRepository.UpdateAsync(detail);
//    }
//    total += detail.Price;
//}

//var billing = await _unitOfWork.BillingRepository.GetAsync(b => b.ReservationId == orders[i].ReservationId);
//var result = new Billing();
//if (billing is null)
//{
//    Billing bill = new Billing
//    {
//        Id = "demo-" + orders[i].Id,
//        ReservationId = orders[i].ReservationId,
//        OrderId = orders[i].Id,
//        OrderAmount = total
//    };
//    result = await _unitOfWork.BillingRepository.InsertAsync(bill);
//}
//else
//{
//    billing.OrderAmount = total - billing.ReservationAmount;
//    result = await _unitOfWork.BillingRepository.UpdateAsync(billing);
//}

//await _unitOfWork.CompleteAsync(cancellationToken);
//dto.updated.Add(orders[i].Id);