﻿using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Mappings;
using Application.Models;
using Application.OrderDetails.Response;
using Application.Orders.Events;
using Application.Orders.Helpers;
using Application.Orders.Response;
using AutoMapper;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Orders.Commands
{
    public sealed class CreatePreOrderFoodForReservationCommand : IMapFrom<Order>, IRequest<Response<OrderDto>>
    {
        public int ReservationId { get; set; }
        public Dictionary<int, FoodInfo> OrderDetails { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<CreatePreOrderFoodForReservationCommand, Order>()
                .ForSourceMember(dto => dto.OrderDetails, opt => opt.DoNotValidate());
        }
    }

    public sealed class CreatePreOrderFoodForReservationCommandHandler : IRequestHandler<CreatePreOrderFoodForReservationCommand, Response<OrderDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IDateTime _dateTime;

        public CreatePreOrderFoodForReservationCommandHandler(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IMapper mapper, IDateTime dateTime)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
            _dateTime = dateTime;
        }

        public async Task<Response<OrderDto>> Handle(CreatePreOrderFoodForReservationCommand request, CancellationToken cancellationToken)
        {
            //var user = await _userManager.Users.FirstOrDefaultAsync(e => e.UserName.Equals("defaultCustomer"), cancellationToken);
            var availableMenu = await _unitOfWork.MenuRepository.GetAsync(e => e.Available);
            if (availableMenu is null)
            {
                throw new NotFoundException($"No available {nameof(Menu)}");
            }

            var reservation = await _unitOfWork.ReservationRepository.GetAsync(e => e.Id == request.ReservationId, includeProperties: $"{nameof(Reservation.ReservationTables)}");
            if (reservation is null)
            {
                throw new NotFoundException(nameof(Reservation), $"with reservation id {request.ReservationId}");
            }

            var user = await _userManager.FindByIdAsync(reservation.UserId);

            var entity = new Order
            {
                Id = $"{reservation.Id}-{user.PhoneNumber}-{_dateTime.Now.ToString("dd-MM-yyyy-HH:mm:ss")}",
                ReservationId = reservation.Id,
                Date = _dateTime.Now,
                Status = OrderStatus.Reserved,
                OrderDetails = new List<OrderDetail>(),
            };

            //Comment until online payment complete
            var tableType = await _unitOfWork.TableTypeRepository.GetAsync(e => !e.IsDeleted && e.Id == reservation.TableTypeId);
            if (tableType is null)
            {
                throw new NotFoundException(nameof(TableType), reservation.TableTypeId);
            }

            var billing = await _unitOfWork.BillingRepository.GetAsync(b => b.ReservationId == reservation.Id);
            if (billing is not null)
            {
                entity.PrePaid = billing.ReservationAmount;
            }
            else
            {
                entity.PrePaid = 0;
            }

            foreach (var dish in request.OrderDetails)
            {
                for (int i = 0; i < dish.Value.Quantity; i++)
                {
                    var food = await _unitOfWork.MenuFoodRepository.GetAsync(e => e.FoodId == dish.Key && e.MenuId == availableMenu.Id);
                    if (food is null)
                    {
                        throw new NotFoundException(nameof(Food), dish.Key);
                    }
                    entity.OrderDetails.Add(new OrderDetail
                    {
                        FoodId = dish.Key,
                        Price = food.Price,
                        Note = string.IsNullOrWhiteSpace(dish.Value.Note) ? string.Empty : dish.Value.Note,
                        Status = OrderDetailStatus.Reserved
                    });
                }
            }
            var result = await _unitOfWork.OrderRepository.InsertAsync(entity);
            entity.AddDomainEvent(new CreateOrderEvent
            {
                id = entity.Id
            });
            reservation.IsPriorFoodOrder = true;
            await _unitOfWork.ReservationRepository.UpdateAsync(reservation);
            await _unitOfWork.CompleteAsync(cancellationToken);
            if (result is null)
            {
                return new Response<OrderDto>("error");
            }

            var mappedResult = _mapper.Map<OrderDto>(result);
            mappedResult.FullName = user.FullName;
            mappedResult.PhoneNumber = user.PhoneNumber;

            var orderDetails = new List<OrderDetailDto>();
            double total = 0;
            foreach (var detail in result.OrderDetails)
            {
                var element = orderDetails.FirstOrDefault(e => e.FoodId.Equals(detail.FoodId));
                if (element is null)
                {
                    var food = await _unitOfWork.FoodRepository.GetAsync(e => e.Id == detail.FoodId);
                    orderDetails.Add(new OrderDetailDto
                    {
                        OrderId = result.Id,
                        FoodId = detail.FoodId,
                        FoodName = food.Name,
                        Status = OrderDetailStatus.Reserved,
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
                total += detail.Price;
            }
            total -= result.PrePaid;
            mappedResult.Total = total;
            mappedResult.OrderDetails = orderDetails;
            return new Response<OrderDto>(mappedResult)
            {
                StatusCode = System.Net.HttpStatusCode.Created
            };
        }
    }
}
