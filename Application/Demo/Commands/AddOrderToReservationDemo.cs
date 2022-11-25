using Application.Common.Exceptions;
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
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.Commands
{
    public sealed class AddOrderToReservationDemo : IMapFrom<Order>, IRequest<Response<OrderDto>>
    {
        public List<int> ReservationIds { get; set; }
        public Dictionary<int, FoodInfo> OrderDetails { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<AddOrderToReservationDemo, Order>()
                .ForSourceMember(dto => dto.OrderDetails, opt => opt.DoNotValidate());
        }
    }

    public sealed class AddOrderToReservationDemoHandler : IRequestHandler<AddOrderToReservationDemo, Response<OrderDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IDateTime _dateTime;

        public AddOrderToReservationDemoHandler(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IMapper mapper, IDateTime dateTime)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
            _dateTime = dateTime;
        }

        public async Task<Response<OrderDto>> Handle(AddOrderToReservationDemo request, CancellationToken cancellationToken)
        {
            foreach (int reservationId in request.ReservationIds.ToList())
            {
                
                var availableMenu = await _unitOfWork.MenuRepository.GetAsync(e => e.Available);
                if (availableMenu is null)
                {
                    throw new NotFoundException($"No available {nameof(Menu)}");
                }

                var reservation = await _unitOfWork.ReservationRepository.GetAsync(e => e.Id == reservationId && e.Status == ReservationStatus.CheckIn, includeProperties: $"{nameof(Reservation.ReservationTables)}");
                if (reservation is null)
                {
                    throw new NotFoundException(nameof(Reservation), $"with reservation {reservationId}");
                }

                var user = await _userManager.Users.FirstOrDefaultAsync(e => e.Id.Equals(reservation.UserId), cancellationToken);

                var entity = new Order
                {
                    Id = $"demo-{reservation.ReservationTables[0].TableId}-{user.PhoneNumber}-{_dateTime.Now.ToString("dd-MM-yyyy-HH:mm:ss")}",
                    UserId = user.Id,
                    ReservationId = reservation.Id,
                    Date = _dateTime.Now,
                    Status = OrderStatus.Processing,
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
                            Status = OrderDetailStatus.Received
                        });
                    }
                }
                var result = await _unitOfWork.OrderRepository.InsertAsync(entity);
                await _unitOfWork.CompleteAsync(cancellationToken);
                if (result is null)
                {
                    return new Response<OrderDto>("error");
                }
            }
            return new Response<OrderDto>("success")
            {
                StatusCode = System.Net.HttpStatusCode.Created
            };
        }
    }
}
