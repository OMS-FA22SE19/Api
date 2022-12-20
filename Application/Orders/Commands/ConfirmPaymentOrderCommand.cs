using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Models;
using Application.OrderDetails.Response;
using Application.Orders.Response;
using AutoMapper;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using System.Linq.Expressions;

namespace Application.Orders.Commands
{
    public sealed class ConfirmPaymentOrderCommand : IRequest<Response<OrderDto>>
    {
        public string Id { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ConfirmPaymentOrderCommand, Order>();
        }
    }

    public sealed class ConfirmPaymentOrderCommandHandler : IRequestHandler<ConfirmPaymentOrderCommand, Response<OrderDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTime _dateTime;

        public ConfirmPaymentOrderCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService, IDateTime dateTime)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _dateTime = dateTime;
        }

        public async Task<Response<OrderDto>> Handle(ConfirmPaymentOrderCommand request, CancellationToken cancellationToken)
        {
            var entity = await _unitOfWork.OrderRepository.GetAsync(e => e.Id == request.Id, $"{nameof(Order.OrderDetails)},{nameof(Order.Reservation)}");
            if (entity is null)
            {
                throw new NotFoundException(nameof(Order), request.Id);
            }

            if (_currentUserService.UserId is not null)
            {
                if (_currentUserService.Role.Equals("Customer"))
                {
                    if (!_currentUserService.UserName.Equals("defaultCustomer"))
                    {
                        if (!_currentUserService.UserId.Equals(entity.Reservation.UserId))
                        {
                            throw new BadRequestException("This is not your order");
                        }
                    }
                }
            }

            foreach (var detail in entity.OrderDetails)
            {
                if (detail.Status == OrderDetailStatus.Processing)
                {
                    detail.Status = OrderDetailStatus.Overcharged;
                    await _unitOfWork.OrderDetailRepository.UpdateAsync(detail);
                }
            }

            var reservation = await _unitOfWork.ReservationRepository.GetAsync(r => r.Id == entity.ReservationId, $"{nameof(Reservation.ReservationTables)}");
            foreach (var rt in reservation.ReservationTables)
            {
                var table = await _unitOfWork.TableRepository.GetAsync(t => t.Id == rt.TableId);
                table.Status = TableStatus.Available;
                await _unitOfWork.TableRepository.UpdateAsync(table);
            }


            MapToEntity(entity);
            var result = await _unitOfWork.OrderRepository.UpdateAsync(entity);
            await _unitOfWork.CompleteAsync(cancellationToken);

            entity.Reservation.Status = ReservationStatus.Done;
            await _unitOfWork.ReservationRepository.UpdateAsync(entity.Reservation);
            await _unitOfWork.CompleteAsync(cancellationToken);

            List<Expression<Func<OrderDetail, bool>>> filters = new()
            {
                e => e.OrderId.Equals(result.Id)
            };
            var detailInDatabase = await _unitOfWork.OrderDetailRepository.GetAllAsync(filters, includeProperties: $"{nameof(OrderDetail.Food)}");
            if (result is null)
            {
                return new Response<OrderDto>("error");
            }

            double total = 0;
            List<OrderDetailDto> orderDetails = new();
            foreach (var detail in detailInDatabase)
            {
                if (detail.Status != OrderDetailStatus.Cancelled)
                {
                    var element = orderDetails.FirstOrDefault(e => e.FoodId.Equals(detail.FoodId));
                    if (element is null)
                    {
                        orderDetails.Add(new OrderDetailDto
                        {
                            OrderId = result.Id,
                            UserId = result.Reservation.UserId,
                            Date = result.Date,
                            FoodId = detail.FoodId,
                            FoodName = detail.Food.Name,
                            Status = detail.Status,
                            Quantity = 1,
                            Price = detail.Price,
                            Amount = detail.Price
                        });
                    }
                    else
                    {
                        element.Quantity += 1;
                        element.Amount += detail.Price;
                    }
                    total += detail.Price;
                }
            }
            total -= entity.PrePaid;

            if(_currentUserService.Role is not null)
            {
                if (!"Customer".Equals(_currentUserService.Role))
                {
                    var billing = await _unitOfWork.BillingRepository.GetAsync(e => e.ReservationId == entity.ReservationId);
                    if (billing is not null)
                    {
                        billing.OrderEBillingId = $"{entity.Id}-{_dateTime.Now:yyyyMMddHHmmss}";
                        billing.OrderId = entity.Id;
                        billing.OrderAmount = total;
                        await _unitOfWork.BillingRepository.UpdateAsync(billing);
                        await _unitOfWork.CompleteAsync(cancellationToken);
                    }
                }
            }
            
            var bill = new OrderDto()
            {
                Id = entity.Id,
                Date = entity.Date,
                PhoneNumber = entity.Reservation.PhoneNumber,
                Status = OrderStatus.Paid,
                OrderDetails = orderDetails,
                PrePaid = entity.PrePaid,
                Total = total
            };
            return new Response<OrderDto>(bill);
        }

        private void MapToEntity(Order entity)
        {
            entity.Status = OrderStatus.Paid;
        }
    }
}
