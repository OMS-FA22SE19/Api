using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Mappings;
using Application.Demo.Responses;
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
using System.Linq.Expressions;

namespace Application.Orders.Commands
{
    public sealed class AddOrderToReservationDemo : IRequest<Response<OrderReservationDemoDto>>
    {
        public List<int> ReservationIds { get; set; }
        //public Dictionary<int, FoodInfo> OrderDetails { get; set; }

        //public void Mapping(Profile profile)
        //{
        //    profile.CreateMap<AddOrderToReservationDemo, Order>()
        //        .ForSourceMember(dto => dto.OrderDetails, opt => opt.DoNotValidate());
        //}
    }

    public sealed class AddOrderToReservationDemoHandler : IRequestHandler<AddOrderToReservationDemo, Response<OrderReservationDemoDto>>
    {
        static Random rnd = new Random();
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

        public async Task<Response<OrderReservationDemoDto>> Handle(AddOrderToReservationDemo request, CancellationToken cancellationToken)
        {
            var availableMenu = await _unitOfWork.MenuRepository.GetAsync(e => e.Available);
            if (availableMenu is null)
            {
                throw new NotFoundException($"No available {nameof(Menu)}");
            }

            List<Expression<Func<Food, bool>>> filters = new();
            filters.Add(e => e.MenuFoods.Any(m => m.MenuId == availableMenu.Id));
            var availableFood = await _unitOfWork.FoodRepository.GetAllAsync(filters, null, "");

            OrderReservationDemoDto dto = new OrderReservationDemoDto()
            {
                created = new List<string>(),
                Error = new List<string>(),
                TotalDishAdded = ""
            };
            int totalDishes = 0;
            foreach (int reservationId in request.ReservationIds.ToList())
            {
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

                int randomQuantity = rnd.Next(1, 5);
                for(int i = 0; i < randomQuantity; i++)
                {
                    int r = rnd.Next(availableFood.Count());
                    var food = availableFood[r];
                    var foodInMenu = await _unitOfWork.MenuFoodRepository.GetAsync(e => e.FoodId == food.Id && e.MenuId == availableMenu.Id);

                    entity.OrderDetails.Add(new OrderDetail
                    {
                        FoodId = food.Id,
                        Price = foodInMenu.Price,
                        Note = "For Demo",
                        Status = OrderDetailStatus.Received
                    });
                }

                totalDishes += randomQuantity;

                var result = await _unitOfWork.OrderRepository.InsertAsync(entity);
                await _unitOfWork.CompleteAsync(cancellationToken);
                if (result is null)
                {
                    return new Response<OrderReservationDemoDto>("error");
                }

                dto.created.Add(entity.Id);
            }
            dto.TotalDishAdded = $"Added {totalDishes}";
            return new Response<OrderReservationDemoDto>(dto)
            {
                StatusCode = System.Net.HttpStatusCode.Created
            };
        }
    }
}
