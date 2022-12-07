using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Models;
using Application.OrderDetails.Response;
using Application.Orders.Commands;
using Application.Orders.Helpers;
using Application.Orders.Response;
using AutoMapper;
using Core.Common;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using Firebase.Auth;
using Microsoft.AspNetCore.Identity;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;
using System.Net;

namespace Application.UnitTests.Orders.Commands
{
    [TestFixture]
    public class CreateOrderCommandHandlerTest
    {
        private List<Order> _Orders;
        private List<OrderDetail> _OrderDetails;
        private List<Reservation> _Reservations;
        private List<ReservationTable> _ReservationTables;
        private List<Table> _Tables;
        private List<TableType> _TableTypes;
        private List<Billing> _Billing;
        private List<ApplicationUser> _Users;
        private List<Food> _Foods;
        private List<Menu> _Menus;
        private List<MenuFood> _MenuFoods;
        private UserManager<ApplicationUser> _UserManager;
        private IOrderRepository _OrderRepository;
        private IOrderDetailRepository _OrderDetailRepository;
        private IReservationRepository _ReservationRepository;
        private ITableRepository _TableRepository;
        private ITableTypeRepository _TableTypeRepository;
        private IReservationTableRepository _ReservationTableRepository;
        private IBillingRepository _BillingRepository;
        private IFoodRepository _FoodRepository;
        private IMenuRepository _MenuRepository;
        private IMenuFoodRepository _MenuFoodRepository;
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;
        private IDateTime _datetime;

        [OneTimeSetUp]
        public void SetUp()
        {
            _Orders = DataSource.Orders;
            _OrderDetails = DataSource.OrderDetails;
            _Reservations = DataSource.Reservations;
            _ReservationTables = DataSource.ReservationTables;
            _Tables = DataSource.Tables;
            _TableTypes = DataSource.TableTypes;
            _Billing = DataSource.Billings;
            _Users = DataSource.Users;
            _Foods = DataSource.Foods;
            _Menus= DataSource.Menus;
            _MenuFoods= DataSource.MenuFoods;
            foreach (Menu menu in _Menus)
            {
                menu.MenuFoods = _MenuFoods.FindAll(od => od.MenuId.Equals(menu.Id));
                if (menu.MenuFoods is not null)
                {
                    foreach (MenuFood menuFood in menu.MenuFoods)
                    {
                        menuFood.Food = _Foods.Find(t => t.Id == menuFood.FoodId);
                    }
                }
            }
            foreach (Order order in _Orders)
            {
                order.User = _Users.Find(ft => ft.Id == order.UserId);
                order.OrderDetails = _OrderDetails.FindAll(od => od.OrderId.Equals(order.Id));
                if (order.OrderDetails is not null)
                {
                    foreach (OrderDetail detail in order.OrderDetails)
                    {
                        detail.Food = _Foods.Find(t => t.Id == detail.FoodId);
                    }
                }
            }
            foreach (Reservation reservation in _Reservations)
            {
                reservation.ReservationTables = _ReservationTables.FindAll(ft => ft.ReservationId == reservation.Id);
                if (reservation.ReservationTables != null)
                {
                    foreach (ReservationTable type in reservation.ReservationTables)
                    {
                        type.Table = _Tables.Find(t => t.Id == type.TableId);
                    }
                }
            }
        }

        [SetUp]
        public void ReInitializeTest()
        {
            _Orders = DataSource.Orders;
            _OrderDetails = DataSource.OrderDetails;
            _Reservations = DataSource.Reservations;
            _ReservationTables = DataSource.ReservationTables;
            _Tables = DataSource.Tables;
            _TableTypes = DataSource.TableTypes;
            _Billing = DataSource.Billings;
            _Users = DataSource.Users;
            _Foods = DataSource.Foods;
            _MenuFoods = DataSource.MenuFoods;

            foreach (Menu menu in _Menus)
            {
                menu.MenuFoods = _MenuFoods.FindAll(od => od.MenuId.Equals(menu.Id));
                if (menu.MenuFoods is not null)
                {
                    foreach (MenuFood menuFood in menu.MenuFoods)
                    {
                        menuFood.Food = _Foods.Find(t => t.Id == menuFood.FoodId);
                    }
                }
            }
            foreach (Order order in _Orders)
            {
                order.User = _Users.Find(ft => ft.Id == order.UserId);
                order.OrderDetails = _OrderDetails.FindAll(od => od.OrderId.Equals(order.Id));
                if (order.OrderDetails is not null)
                {
                    foreach (OrderDetail detail in order.OrderDetails)
                    {
                        detail.Food = _Foods.Find(t => t.Id == detail.FoodId);
                    }
                }
            }
            foreach (Reservation reservation in _Reservations)
            {
                reservation.ReservationTables = _ReservationTables.FindAll(ft => ft.ReservationId == reservation.Id);
                if (reservation.ReservationTables != null)
                {
                    foreach (ReservationTable type in reservation.ReservationTables)
                    {
                        type.Table = _Tables.Find(t => t.Id == type.TableId);
                    }
                }
            }
            _UserManager = MockUserManager(_Users).Object;
            _OrderRepository = SetUpOrderRepository();
            _OrderDetailRepository = SetUpOrderDetailRepository();
            _MenuRepository = SetUpMenuRepository();
            _ReservationRepository = SetUpReservationRepository();
            _TableRepository = SetUpTableRepository();
            _ReservationTableRepository = SetUpReservationTableRepository();
            _TableTypeRepository = SetUpTableTypeRepository();
            _BillingRepository = SetUpBillingRepository();
            _MenuFoodRepository = SetUpMenuFoodRepository();
            _FoodRepository = SetUpFoodRepository();
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(x => x.OrderRepository).Returns(_OrderRepository);
            unitOfWork.SetupGet(x => x.OrderDetailRepository).Returns(_OrderDetailRepository);
            unitOfWork.SetupGet(x => x.ReservationRepository).Returns(_ReservationRepository);
            unitOfWork.SetupGet(x => x.ReservationTableRepository).Returns(_ReservationTableRepository);
            unitOfWork.SetupGet(x => x.TableRepository).Returns(_TableRepository);
            unitOfWork.SetupGet(x => x.TableTypeRepository).Returns(_TableTypeRepository);
            unitOfWork.SetupGet(x => x.BillingRepository).Returns(_BillingRepository);
            unitOfWork.SetupGet(x => x.FoodRepository).Returns(_FoodRepository);
            unitOfWork.SetupGet(x => x.MenuRepository).Returns(_MenuRepository);
            unitOfWork.SetupGet(x => x.MenuFoodRepository).Returns(_MenuFoodRepository);
            _unitOfWork = unitOfWork.Object;
            _mapper = SetUpMapper();
            _datetime = SetUpDatetime();
        }

        [TearDown]
        public void DisposeTest()
        {
            _OrderRepository = null;
            _OrderDetailRepository = null;
            _ReservationRepository = null;
            _TableRepository = null;
            _ReservationTableRepository = null;
            _FoodRepository = null;
            _unitOfWork = null;
        }

        [OneTimeTearDown]
        public void DisposeAllObjects()
        {
            _Foods = null;
            _Users = null;
            _Orders = null;
            _OrderDetails = null;
            _Reservations = null;
            _ReservationTables = null;
            _TableTypes = null;
            _Tables = null;
        }

        #region Unit Tests
        [TestCase(4)]
        public async Task Should_Create_Order(int reservationId, Dictionary<int, FoodInfo> OrderDetails = null)
        {
            //Arrange
            Dictionary<int, FoodInfo> orderDetails = new Dictionary<int, FoodInfo>();
            orderDetails.Add(1, new FoodInfo() { Quantity = 1, Note = "fast" });
            var request = new CreateOrderCommand()
            {
                ReservationId= reservationId,
                OrderDetails = orderDetails
            };

            var handler = new CreateOrderCommandHandler(_unitOfWork, _UserManager, _mapper, _datetime);

            var reservation = _Reservations.Find(t => t.Id == reservationId);
            var user = _Users.Find(ft => ft.Id == reservation.UserId);
            var expected = new Response<OrderDto>(new OrderDto
            {
                Id = _Orders.Max(e => e.Id) + 1,
                UserId = user.Id,
                Date = _datetime.Now,
                Status = OrderStatus.Processing,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                OrderDetails = new List<OrderDetailDto>()
            })
            {
                StatusCode = HttpStatusCode.Created
            };

            var billing = _Billing.Find(b => b.ReservationId== reservationId);
            if (billing is not null)
            {
                expected.Data.PrePaid = billing.ReservationAmount;
            }
            else
            {
                expected.Data.PrePaid = 0;
            }
            double total = 0;
            List<OrderDetailDto> list = new List<OrderDetailDto>();
            foreach (var detail in orderDetails)
            {
                var food = _Foods.Find(f => f.Id == detail.Key);
                var element = list.FirstOrDefault(e => e.FoodId.Equals(detail.Key));
                if (element == null)
                {
                    list.Add(new OrderDetailDto
                    {
                        OrderId = expected.Data.Id,
                        UserId = expected.Data.UserId,
                        Date = expected.Data.Date,
                        FoodId = food.Id,
                        FoodName = food.Name,
                        Status = OrderDetailStatus.Received,
                        Quantity = 1,
                        Price = 100000,
                        Amount = 100000
                    });
                }
                else
                {
                    element.Quantity += 1;
                    element.Amount += 100000;
                }
                total += 100000;
            }

            expected.Data.OrderDetails= list;
            expected.Data.Total = total - expected.Data.PrePaid;
            var count = _Orders.Count + 1;
            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.That(actual.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(_Orders.Count, Is.EqualTo(count));
            var inDatabase = _Orders.FirstOrDefault(e => e.ReservationId == reservationId);
            Assert.NotNull(inDatabase);
            Assert.That(actual.Data, Is.EqualTo(expected.Data));
        }

        [TestCase(10)]
        public async Task Should_Return_Throw_NotFound_Exception(int reservationId)
        {
            //Arrange
            Dictionary<int, FoodInfo> orderDetails = new Dictionary<int, FoodInfo>();
            orderDetails.Add(1, new FoodInfo() { Quantity = 1, Note = "fast" });
            var request = new CreateOrderCommand()
            {
                ReservationId = reservationId,
                OrderDetails = orderDetails
            };
            var handler = new CreateOrderCommandHandler(_unitOfWork, _UserManager, _mapper, _datetime);

            //Act
            var ex = Assert.ThrowsAsync<NotFoundException>(async () => await handler.Handle(request, CancellationToken.None));

            //Assert
            Assert.That(ex.Message, Is.EqualTo($"Entity Reservation (with reservation {request.ReservationId}) was not found."));
        }
        #endregion Unit Tests

        #region Private member methods
        private IOrderRepository SetUpOrderRepository()
        {
            var mockOrderRepository = new Mock<IOrderRepository>();
            mockOrderRepository.Setup(m => m.InsertAsync(It.IsAny<Order>()))
                .ReturnsAsync(
                (Order Order)
                =>
                {
                    Order.Id = _Orders.Max(e => e.Id) + 1;
                    var count = _Orders.Count;
                    _Orders.Add(Order);
                    foreach(OrderDetail detail in _Orders[count].OrderDetails)
                    {
                        detail.Id = _OrderDetails.Max(e => e.Id) + 1;
                        _OrderDetails.Add(detail);
                    }
                    return Order;
                });
            return mockOrderRepository.Object;
        }

        private Mock<UserManager<ApplicationUser>> MockUserManager(List<ApplicationUser> ls)
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            var mgr = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
            mgr.Object.UserValidators.Add(new UserValidator<ApplicationUser>());
            mgr.Object.PasswordValidators.Add(new PasswordValidator<ApplicationUser>());

            mgr.Setup(x => x.DeleteAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);
            mgr.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success).Callback<ApplicationUser, string>((x, y) => ls.Add(x));
            mgr.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);
            var mock = ls.AsQueryable().BuildMock();

            mgr.Setup(x => x.Users).Returns(ls.AsQueryable().BuildMock());

            mgr.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(
                (string UserId)
                =>
                {
                    return _Users.AsQueryable().FirstOrDefault(e => e.Id.Equals(UserId));
                });

            return mgr;
        }

        private IOrderDetailRepository SetUpOrderDetailRepository()
        {
            var mockOrderDetailRepository = new Mock<IOrderDetailRepository>();
            mockOrderDetailRepository.Setup(m => m.GetAsync(It.IsAny<Expression<Func<OrderDetail, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(
                (Expression<Func<OrderDetail, bool>> filter,
                string includeString)
                =>
                {
                    return _OrderDetails.AsQueryable().FirstOrDefault(filter);
                });
            return mockOrderDetailRepository.Object;
        }

        private IMenuFoodRepository SetUpMenuFoodRepository()
        {
            var mockMenuFoodRepository = new Mock<IMenuFoodRepository>();
            mockMenuFoodRepository.Setup(m => m.GetAsync(It.IsAny<Expression<Func<MenuFood, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(
                (Expression<Func<MenuFood, bool>> filter,
                string includeString)
                =>
                {
                    return _MenuFoods.AsQueryable().FirstOrDefault(filter);
                });
            return mockMenuFoodRepository.Object;
        }
        private IReservationRepository SetUpReservationRepository()
        {
            var mockReservationRepository = new Mock<IReservationRepository>();
            mockReservationRepository.Setup(m => m.GetAsync(It.IsAny<Expression<Func<Reservation, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(
                (Expression<Func<Reservation, bool>> filter,
                string includeString)
                =>
                {
                    return _Reservations.AsQueryable().FirstOrDefault(filter);
                });
            return mockReservationRepository.Object;
        }

        private IReservationTableRepository SetUpReservationTableRepository()
        {
            var mockReservationTableRepository = new Mock<IReservationTableRepository>();
            mockReservationTableRepository.Setup(m => m.GetAsync(It.IsAny<Expression<Func<ReservationTable, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(
                (Expression<Func<ReservationTable, bool>> filter,
                string includeString)
                =>
                {
                    return _ReservationTables.AsQueryable().FirstOrDefault(filter);
                });
            return mockReservationTableRepository.Object;
        }

        private ITableRepository SetUpTableRepository()
        {
            var mockTableRepository = new Mock<ITableRepository>();
            mockTableRepository.Setup(m => m.GetAsync(It.IsAny<Expression<Func<Table, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(
                (Expression<Func<Table, bool>> filter,
                string includeString)
                =>
                {
                    return _Tables.AsQueryable().FirstOrDefault(filter);
                });
            return mockTableRepository.Object;
        }

        private ITableTypeRepository SetUpTableTypeRepository()
        {
            var mockTableTypeRepository = new Mock<ITableTypeRepository>();
            mockTableTypeRepository.Setup(m => m.GetAsync(It.IsAny<Expression<Func<TableType, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(
                (Expression<Func<TableType, bool>> filter,
                string includeString)
                =>
                {
                    return _TableTypes.AsQueryable().FirstOrDefault(filter);
                });
            return mockTableTypeRepository.Object;
        }

        private IBillingRepository SetUpBillingRepository()
        {
            var mockBillingRepository = new Mock<IBillingRepository>();
            mockBillingRepository.Setup(m => m.GetAsync(It.IsAny<Expression<Func<Billing, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(
                (Expression<Func<Billing, bool>> filter,
                string includeString)
                =>
                {
                    return _Billing.AsQueryable().FirstOrDefault(filter);
                });
            return mockBillingRepository.Object;
        }

        private IFoodRepository SetUpFoodRepository()
        {
            var mockFoodRepository = new Mock<IFoodRepository>();
            mockFoodRepository.Setup(m => m.GetAsync(It.IsAny<Expression<Func<Food, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(
                (Expression<Func<Food, bool>> filter,
                string includeString)
                =>
                {
                    return _Foods.AsQueryable().FirstOrDefault(filter);
                });
            return mockFoodRepository.Object;
        }

        private IMenuRepository SetUpMenuRepository()
        {
            var mockMenuRepository = new Mock<IMenuRepository>();
            mockMenuRepository.Setup(m => m.GetAsync(It.IsAny<Expression<Func<Menu, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(
                (Expression<Func<Menu, bool>> filter,
                string includeString)
                =>
                {
                    return _Menus.AsQueryable().FirstOrDefault(filter);
                });
            return mockMenuRepository.Object;
        }

        private IMapper SetUpMapper()
        {
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<Order>(It.IsAny<CreateOrderCommand>()))
                .Returns((CreateOrderCommand command) => new Order
                {
                    ReservationId = command.ReservationId

                });
            mapperMock.Setup(m => m.Map<OrderDto>(It.IsAny<Order>()))
                .Returns((Order Order) => new OrderDto
                {
                    Id = Order.Id,
                    UserId = Order.UserId,
                    Date = Order.Date,
                    PrePaid = Order.PrePaid,
                    Status = Order.Status,
                    NumOfEdits = Order.NumOfEdits,
                    //FullName = Order.User.FullName,
                    //PhoneNumber = Order.User.PhoneNumber
                });
            return mapperMock.Object;
        }

        private IDateTime SetUpDatetime() 
        { 
            var dateTimeMock = new Mock<IDateTime>();
            dateTimeMock.Setup(d => d.Now).Returns(DateTime.UtcNow.AddHours(7));
            return dateTimeMock.Object;
        }
        #endregion

    }
}
