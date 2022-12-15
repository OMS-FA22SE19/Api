using Application.Common.Exceptions;
using Application.Models;
using Application.OrderDetails.Queries;
using Application.OrderDetails.Response;
using Application.Tables.Response;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;
using System.Net;

namespace Application.UnitTests.Orders.Queries
{
    [TestFixture]
    public class GetOrderDetailWithIdQueryHandlerTest
    {
        private List<Order> _Orders;
        private List<OrderDetail> _OrderDetails;
        private List<Reservation> _Reservations;
        private List<ReservationTable> _ReservationTables;
        private List<Table> _Tables;
        private List<TableType> _TableTypes;
        private List<ApplicationUser> _Users;
        private List<Food> _Foods;
        private IOrderRepository _OrderRepository;
        private IOrderDetailRepository _OrderDetailRepository;
        private IReservationRepository _ReservationRepository;
        private ITableRepository _TableRepository;
        private ITableTypeRepository _TableTypeRepository;
        private IReservationTableRepository _ReservationTableRepository;
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
            _Users = DataSource.Users;
            _Foods = DataSource.Foods;
            foreach (OrderDetail detail in _OrderDetails)
            {
                detail.Order = _Orders.Find(ft => ft.Id == detail.OrderId);
                detail.Food = _Foods.Find(t => t.Id == detail.FoodId);
                if (detail.Order is not null)
                {
                    detail.Order.Reservation.User = _Users.Find(t => t.Id == detail.Order.Reservation.UserId);
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
            _Users = DataSource.Users;
            _Foods = DataSource.Foods;
            foreach (OrderDetail detail in _OrderDetails)
            {
                detail.Order = _Orders.Find(ft => ft.Id == detail.OrderId);
                detail.Food = _Foods.Find(t => t.Id == detail.FoodId);
                if (detail.Order is not null)
                {
                    detail.Order.Reservation.User = _Users.Find(t => t.Id == detail.Order.Reservation.UserId);
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
            _FoodRepository = SetUpFoodRepository();
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(x => x.OrderRepository).Returns(_OrderRepository);
            unitOfWork.SetupGet(x => x.OrderDetailRepository).Returns(_OrderDetailRepository);
            unitOfWork.SetupGet(x => x.ReservationRepository).Returns(_ReservationRepository);
            unitOfWork.SetupGet(x => x.ReservationTableRepository).Returns(_ReservationTableRepository);
            unitOfWork.SetupGet(x => x.TableRepository).Returns(_TableRepository);
            unitOfWork.SetupGet(x => x.TableTypeRepository).Returns(_TableTypeRepository);
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
        [TestCase(1)]
        [TestCase(2)]
        public async Task Should_Return_Order(int id)
        {
            //Arrange
            var request = new GetOrderDetailWithIdQuery()
            {
                Id = id
            };
            var handler = new GetOrderDetailWithIdQueryHandler(_unitOfWork, _mapper);

            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            var inDatabase = _OrderDetails.FirstOrDefault(x => x.Id == id);
            Assert.NotNull(inDatabase);

            var expected = new Response<DishDto>(new DishDto
            {
                Id = inDatabase.Id,
                UserId = inDatabase.Order.Reservation.UserId,
                Date = inDatabase.Order.Date,
                OrderId = inDatabase.OrderId,
                Status = inDatabase.Status,
                PhoneNumber = inDatabase.Order.Reservation.User.PhoneNumber,
                TableId = "0",
                FoodId = inDatabase.FoodId,
                Note = inDatabase.Note,
                FoodName = inDatabase.Food.Name,
                Price = inDatabase.Price
            });

            Assert.That(actual.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(expected.Data, Is.EqualTo(actual.Data));
        }

        [TestCase("0")]
        [TestCase("10")]
        public async Task Should_Return_Throw_NotFound_Exception(int id)
        {
            //Arrange
            var request = new GetOrderDetailWithIdQuery()
            {
                Id = id
            };
            var handler = new GetOrderDetailWithIdQueryHandler(_unitOfWork, _mapper);

            //Act
            var ex = Assert.ThrowsAsync<NotFoundException>(async () => await handler.Handle(request, CancellationToken.None));

            //Assert
            Assert.That(ex.Message, Is.EqualTo($"Entity {nameof(OrderDetail)} ({request.Id}) was not found."));
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
            mapperMock.Setup(m => m.Map<DishDto>(It.IsAny<OrderDetail>()))
                .Returns((OrderDetail detail) =>
                    new DishDto
                    {
                        Id = detail.Id,
                        UserId = detail.Order.Reservation.UserId,
                        Date = detail.Order.Date,
                        OrderId = detail.OrderId,
                        Status = detail.Status,
                        PhoneNumber = detail.Order.Reservation.User.PhoneNumber,
                        TableId = "0",
                        FoodId = detail.FoodId,
                        Note = detail.Note,
                        FoodName = detail.Food.Name,
                        Price = detail.Price
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
