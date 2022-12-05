using Application.Common.Exceptions;
using Application.Models;
using Application.Tables.Response;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;
using System.Net;
using Application.Orders.Queries;
using Application.Orders.Response;
using Microsoft.EntityFrameworkCore;
using Application.OrderDetails.Response;
using Core.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Application.UnitTests.Orders.Queries
{
    [TestFixture]
    public class GetOrderWithIdQueryHandlerTest
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
        private IOrderRepository _OrderRepository;
        private IOrderDetailRepository _OrderDetailRepository;
        private IReservationRepository _ReservationRepository;
        private ITableRepository _TableRepository;
        private ITableTypeRepository _TableTypeRepository;
        private IReservationTableRepository _ReservationTableRepository;
        private IBillingRepository _BillingRepository;
        private IFoodRepository _FoodRepository;
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;

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
            _OrderRepository = SetUpOrderRepository();
            _OrderDetailRepository = SetUpOrderDetailRepository();
            _ReservationRepository = SetUpReservationRepository();
            _TableRepository = SetUpTableRepository();
            _ReservationTableRepository = SetUpReservationTableRepository();
            _TableTypeRepository = SetUpTableTypeRepository();
            _BillingRepository = SetUpBillingRepository();
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
            _unitOfWork = unitOfWork.Object;
            _mapper = SetUpMapper();
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
        [TestCase("1")]
        public async Task Should_Return_Order(string id)
        {
            //Arrange
            var request = new GetOrderWithIdQuery()
            {
                Id = id
            };
            var handler = new GetOrderWithIdQueryHandler(_unitOfWork, _mapper);

            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            var inDatabase = _Orders.FirstOrDefault(x => x.Id == id);
            Assert.NotNull(inDatabase);

            var user = _Users.Find(ft => ft.Id == inDatabase.UserId);

            var reservation = _Reservations.Find(r => r.Id == inDatabase.ReservationId);
            var expected = new Response<OrderDto>(new OrderDto
            {
                Id = inDatabase.Id,
                UserId = inDatabase.UserId,
                Date = inDatabase.Date,
                PrePaid = inDatabase.PrePaid,
                Status = inDatabase.Status,
                NumOfEdits = inDatabase.NumOfEdits,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                OrderDetails = new List<OrderDetailDto>()
            });

            double total = 0;
            List<OrderDetailDto> orderDetails = new();
            foreach (var detail in inDatabase.OrderDetails)
            {
                if (detail.Status != OrderDetailStatus.Cancelled)
                {
                    var element = orderDetails.FirstOrDefault(e => e.FoodId.Equals(detail.FoodId));
                    if (element is null)
                    {
                        orderDetails.Add(new OrderDetailDto
                        {
                            OrderId = inDatabase.Id,
                            UserId = inDatabase.UserId,
                            Date = inDatabase.Date,
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
            total -= inDatabase.PrePaid;

            expected.Data.OrderDetails = orderDetails;
            expected.Data.Total = total;
            if (reservation.ReservationTables.Any())
            {
                expected.Data.TableId = reservation.ReservationTables[0].TableId;
            }

            Assert.That(actual.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(expected.Data, Is.EqualTo(actual.Data));
        }

        [TestCase("10")]
        public async Task Should_Return_Throw_NotFound_Exception(string id)
        {
            //Arrange
            var request = new GetOrderWithIdQuery()
            {
                Id = id
            };
            var handler = new GetOrderWithIdQueryHandler(_unitOfWork, _mapper);

            //Act
            var ex = Assert.ThrowsAsync<NotFoundException>(async () => await handler.Handle(request, CancellationToken.None));

            //Assert
            Assert.That(ex.Message, Is.EqualTo($"Entity {nameof(Order)} (with {request.Id}) was not found."));
        }
        #endregion Unit Tests

        #region Private member methods
        private IOrderRepository SetUpOrderRepository()
        {
            var mockOrderRepository = new Mock<IOrderRepository>();
            mockOrderRepository.Setup(m => m.GetAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(
                (Expression<Func<Order, bool>> filter,
                string includeString)
                =>
                {
                    if (!string.IsNullOrWhiteSpace(includeString))
                    {
                        foreach (var includeProperty in includeString.Split
                        (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            return _Orders.AsQueryable().Include(includeProperty).FirstOrDefault(filter);
                        }
                    }
                    return _Orders.AsQueryable().FirstOrDefault(filter);
                });
            return mockOrderRepository.Object;
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

        private IMapper SetUpMapper()
        {
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<OrderDto>(It.IsAny<Order>()))
                .Returns((Order Order) => 
                //(
                //    string fullname = null;
                //    string phoneNumber = null;
                //    if (Order.User != null)
                //    {
                //        fullname = Order.User.FullName;
                //        phoneNumber = Order.User.PhoneNumber;
                //    }
                    new OrderDto
                    {
                        Id = Order.Id,
                        UserId = Order.UserId,
                        Date = Order.Date,
                        PrePaid = Order.PrePaid,
                        Status = Order.Status,
                        NumOfEdits = Order.NumOfEdits,
                        FullName = Order.User.FullName,
                        PhoneNumber = Order.User.PhoneNumber
                    }
                );
            mapperMock.Setup(m => m.Map<TableDto>(It.IsAny<Table>()))
                .Returns((Table type) => new TableDto
                {
                    Id = type.Id,
                    NumOfSeats = type.NumOfSeats,
                    Status = type.Status,
                    TableTypeId = type.TableTypeId
                });
            return mapperMock.Object;
        }
        #endregion

    }
}
