using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Models;
using Application.OrderDetails.Response;
using Application.Orders.Events;
using Application.Reservations.Response;
using AutoMapper;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;

namespace Application.Reservations.Queries
{
    public sealed class CheckinReservationQuery : IRequest<Response<ReservationDto>>
    {
        public int ReservationId { get; set; }
    }

    public sealed class CheckinReservationQueryHandler : IRequestHandler<CheckinReservationQuery, Response<ReservationDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IDateTime _dateTime;
        private readonly ICurrentUserService _currentUserService;

        public CheckinReservationQueryHandler(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IMapper mapper, IDateTime dateTime, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
            _dateTime = dateTime;
            _currentUserService = currentUserService;
        }

        public async Task<Response<ReservationDto>> Handle(CheckinReservationQuery request, CancellationToken cancellationToken)
        {
            var entity = await _unitOfWork.ReservationRepository.GetAsync(e => e.Id == request.ReservationId
                && _dateTime.Now >= e.StartTime.AddMinutes(-15) && _dateTime.Now <= e.EndTime
                && e.Status == ReservationStatus.Reserved
                && !e.IsDeleted,
                    $"{nameof(Reservation.ReservationTables)},{nameof(Reservation.Order)}");
            if (entity is null)
            {
                throw new NotFoundException($"No reservation with id {request.ReservationId} was found");
            }

            if (_currentUserService.Role.Equals("Customer"))
            {
                if (!_currentUserService.UserName.Equals("defaultCustomer"))
                {
                    if (!_currentUserService.UserId.Equals(entity.UserId))
                    {
                        throw new BadRequestException("This is not your reservation");
                    }
                }
            }

            var user = await _userManager.FindByIdAsync(entity.UserId);

            entity.Status = ReservationStatus.CheckIn;

            List<Expression<Func<Table, bool>>> filters = new();
            filters.Add(e => !e.IsDeleted && e.Status == TableStatus.Available && e.NumOfSeats == entity.NumOfSeats && e.TableTypeId == entity.TableTypeId);
            var tables = await _unitOfWork.TableRepository.GetPaginatedListAsync(filters, pageSize: entity.Quantity);
            if (tables.Count < entity.Quantity)
            {
                throw new BadRequestException("There are not enough available table, check again later");
            }
            var tableType = await _unitOfWork.TableTypeRepository.GetAsync(e => !e.IsDeleted && e.Id == entity.TableTypeId);

            var tableIds = new List<int>();

            foreach (var table in tables)
            {
                table.Status = TableStatus.Occupied;

                await _unitOfWork.ReservationTableRepository.InsertAsync(new ReservationTable
                {
                    ReservationId = entity.Id,
                    TableId = table.Id
                });

                tableIds.Add(table.Id);

                await _unitOfWork.TableRepository.UpdateAsync(table);
            }

            List<OrderDetailDto> orderDetailDtos = new List<OrderDetailDto>();
            if (entity.IsPriorFoodOrder)
            {
                entity.Order.Status = OrderStatus.Processing;

                await _unitOfWork.OrderRepository.UpdateAsync(entity.Order);

                List<Expression<Func<OrderDetail, bool>>> orderDetailsFilter = new();
                orderDetailsFilter.Add(od => od.OrderId.Equals(entity.Order.Id));

                var orderDetails = await _unitOfWork.OrderDetailRepository.GetAllAsync(orderDetailsFilter);
                foreach (var orderDetail in orderDetails)
                {
                    orderDetail.Status = OrderDetailStatus.Received;
                    await _unitOfWork.OrderDetailRepository.UpdateAsync(orderDetail);

                    var element = orderDetailDtos.FirstOrDefault(e => e.FoodId.Equals(orderDetail.FoodId));
                    if (element is null)
                    {
                        var food = await _unitOfWork.FoodRepository.GetAsync(e => e.Id == orderDetail.FoodId);
                        orderDetailDtos.Add(new OrderDetailDto
                        {
                            OrderId = entity.Order.Id,
                            FoodId = orderDetail.FoodId,
                            FoodName = food.Name,
                            Status = orderDetail.Status,
                            Quantity = 1,
                            Price = orderDetail.Price,
                            Amount = orderDetail.Price
                        });
                    }
                    else
                    {
                        element.Quantity += 1;
                        element.Amount += orderDetail.Price;
                    }
                }
            }

            var result = await _unitOfWork.ReservationRepository.UpdateAsync(entity);
            entity.AddDomainEvent(new CheckInReservationEvent
            {
                ReservationId = entity.Id,
                tableIds = tableIds,
            });
            await _unitOfWork.CompleteAsync(cancellationToken);
            if (result is null)
            {
                return new Response<ReservationDto>("error");
            }
            var mappedResult = _mapper.Map<ReservationDto>(result);
            mappedResult.OrderDetails = orderDetailDtos;
            var billing = await _unitOfWork.BillingRepository.GetAsync(b => b.ReservationId == entity.Id);
            if (billing is not null)
            {
                mappedResult.PrePaid = billing.ReservationAmount;
            }
            mappedResult.TableType = tableType.Name;
            mappedResult.TableId = $"{tableType.Name}-{tableIds.Min()}";
            return new Response<ReservationDto>(mappedResult);
        }
    }
}
