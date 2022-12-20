﻿using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Models;
using Application.OrderDetails.Response;
using Application.Reservations.Response;
using Application.Users.Response;
using AutoMapper;
using Core.Common;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using System.Linq.Expressions;

namespace Application.Reservations.Queries
{
    public class GetReservationWithPaginationQuery : PaginationRequest, IRequest<Response<PaginatedList<ReservationDto>>>
    {
        public ReservationProperty SearchBy { get; set; }
        public ReservationStatus? Status { get; init; }
    }

    public class GetReservationWithPaginationQueryHandler : IRequestHandler<GetReservationWithPaginationQuery, Response<PaginatedList<ReservationDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTime _dateTime;

        public GetReservationWithPaginationQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService, IDateTime dateTime)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _dateTime = dateTime;
        }

        public async Task<Response<PaginatedList<ReservationDto>>> Handle(GetReservationWithPaginationQuery request, CancellationToken cancellationToken)
        {
            List<Expression<Func<Reservation, bool>>> filters = new();
            Func<IQueryable<Reservation>, IOrderedQueryable<Reservation>> orderBy = null;
            string includeProperties = $"{nameof(Reservation.ReservationTables)}.{nameof(ReservationTable.Table)}.{nameof(Table.TableType)}";

            if (_currentUserService.Role.Equals("Customer"))
            {
                if (!_currentUserService.UserName.Equals("defaultCustomer"))
                {
                    filters.Add(e => e.UserId.Contains(_currentUserService.UserId));
                }
            }

            if (!string.IsNullOrWhiteSpace(request.SearchValue))
            {
                switch (request.SearchBy)
                {
                    case ReservationProperty.FullName:
                        filters.Add(e => e.FullName.Contains(request.SearchValue) || e.User.FullName.Contains(request.SearchValue));
                        break;
                    case ReservationProperty.PhoneNumber:
                        filters.Add(e => e.PhoneNumber.Contains(request.SearchValue) || e.User.PhoneNumber.Contains(request.SearchValue));
                        break;
                    case ReservationProperty.NumOfPeople:
                        if (int.TryParse(request.SearchValue, out var numberOfPeople))
                        {
                            filters.Add(e => e.NumOfPeople == numberOfPeople);
                        }
                        else
                        {
                            filters.Add(e => false);
                        }
                        break;
                    case ReservationProperty.TableType:
                        List<Expression<Func<TableType, bool>>> tableFilters = new();
                        tableFilters.Add(e => e.Name.Contains(request.SearchValue) && !e.IsDeleted);
                        var tableTypes = await _unitOfWork.TableTypeRepository.GetAllAsync(tableFilters, null, null);
                        var tableTypeIds = tableTypes.Select(e => e.Id).ToList();
                        filters.Add(e => tableTypeIds.Contains(e.TableTypeId));
                        break;
                    case ReservationProperty.NumOfSeats:
                        if (int.TryParse(request.SearchValue, out var numOfSeats))
                        {
                            filters.Add(e => e.NumOfSeats == numOfSeats);
                        }
                        else
                        {
                            filters.Add(e => false);
                        }
                        break;
                    default:
                        break;
                }
            }
            if (request.Status is not null)
            {
                filters.Add(e => e.Status == request.Status);
            }

            var result = await _unitOfWork.ReservationRepository.GetPaginatedListAsync(filters, orderBy, includeProperties, request.PageIndex, request.PageSize);

            foreach (var item in result)
            {
                if (item.Created < _dateTime.Now.AddMinutes(-15) && item.Status == ReservationStatus.Available)
                {
                    var bill = await _unitOfWork.BillingRepository.GetAsync(e => e.ReservationId == item.Id);
                    if (bill is null)
                    {
                        item.Status = ReservationStatus.Cancelled;
                        item.ReasonForCancel = "Reservation is have not Paid 15 minutes before create";
                        await _unitOfWork.ReservationRepository.UpdateAsync(item);
                        await _unitOfWork.CompleteAsync(cancellationToken);
                    }
                }
            }


            var mappedResult = _mapper.Map<PaginatedList<Reservation>, PaginatedList<ReservationDto>>(result);
            foreach (var item in mappedResult)
            {
                var tableType = await _unitOfWork.TableTypeRepository.GetAsync(e => e.Id == item.TableTypeId);
                if (tableType is null)
                {
                    throw new NotFoundException(nameof(TableType), item.TableTypeId);
                }
                var orderDetails = new List<OrderDetailDto>();

                var order = await _unitOfWork.OrderRepository.GetAsync(o => o.ReservationId == item.Id, $"{nameof(Order.OrderDetails)}");
                if (order is not null)
                {
                    foreach (var detail in order.OrderDetails)
                    {
                        var element = orderDetails.FirstOrDefault(e => e.FoodId.Equals(detail.FoodId) && e.Status == detail.Status);
                        if (element is null)
                        {
                            var food = await _unitOfWork.FoodRepository.GetAsync(e => e.Id == detail.FoodId);
                            orderDetails.Add(new OrderDetailDto
                            {
                                OrderId = order.Id,
                                FoodId = detail.FoodId,
                                FoodName = food.Name,
                                Status = detail.Status,
                                Quantity = 1,
                                Price = detail.Price,
                                Amount = detail.Price,
                                Note = detail.Note,
                            });
                        }
                        else
                        {
                            element.Quantity += 1;
                            element.Amount += detail.Price;
                        }
                    }
                }
                var billing = await _unitOfWork.BillingRepository.GetAsync(b => b.ReservationId == item.Id);
                if (billing is not null)
                {
                    item.Paid = billing.ReservationAmount;
                }

                item.OrderDetails = orderDetails;
                item.PrePaid = item.NumOfSeats * tableType.ChargePerSeat * item.Quantity;
                item.TableType = tableType.Name;
                var user = await _unitOfWork.UserRepository.GetAsync(u => u.Id.Equals(item.UserId));
                if (user is not null)
                {
                    item.User = _mapper.Map<ApplicationUser, UserDto>(user);
                }
                if (item.ReservationTables?.Any() == true)
                {
                    item.TableId = $"{tableType.Name}-{item.ReservationTables.Min(e => e.TableId)}";
                }
            }
            return new Response<PaginatedList<ReservationDto>>(mappedResult);
        }
    }
}
