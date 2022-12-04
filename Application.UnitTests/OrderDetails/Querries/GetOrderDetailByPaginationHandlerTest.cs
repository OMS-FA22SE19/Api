using Application.Common.Interfaces;
using Application.Models;
using Application.OrderDetails.Queries;
using Application.OrderDetails.Response;
using Application.Orders.Queries;
using Application.Orders.Response;
using Application.Reservations.Response;
using Application.Tables.Response;
using AutoMapper;
using Core.Common;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;
using System.Net;
using System.Web.Mvc;
using static Duende.IdentityServer.Models.IdentityResources;

namespace Application.UnitTests.Orders.Queries
{
    [TestFixture]
    public class GetOrderDetailWithPaginationQueryHandlerTest
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
            _Users = DataSource.Users;
            _Foods = DataSource.Foods;
            foreach (OrderDetail detail in _OrderDetails)
            {
                detail.Order = _Orders.Find(ft => ft.Id == detail.OrderId);
                detail.Food = _Foods.Find(t => t.Id == detail.FoodId);
                if (detail.Order is not null)
                {
                    detail.Order.User = _Users.Find(t => t.Id == detail.Order.UserId);
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
                    detail.Order.User = _Users.Find(t => t.Id == detail.Order.UserId);
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

        [TestCase]
        [TestCase(1, 50, "", null, null, false)]
        [TestCase(1, 2, null, null, null, false)]
        [TestCase(2, 2, null, null, null, false)]
        [TestCase(1, 50, "1", null, null, false)]
        [TestCase(1, 50, "4", null, null, false)]
        [TestCase(1, 50, "", OrderDetailProperty.OrderId, null, false)]
        [TestCase(1, 50, "", OrderDetailProperty.OrderId, null, true)]
        [TestCase(1, 50, "", OrderDetailProperty.FoodId, null, false)]
        [TestCase(1, 50, "", OrderDetailProperty.FoodId, null, true)]
        [TestCase(1, 50, "", OrderDetailProperty.Status, null, false)]
        [TestCase(1, 50, "", OrderDetailProperty.Status, null, true)]
        [TestCase(1, 50, "", OrderDetailProperty.Price, null, false)]
        [TestCase(1, 50, "", OrderDetailProperty.Price, null, true)]
        [TestCase(1, 50, "", null, OrderDetailStatus.Processing, false)]
        [TestCase(1, 50, "", null, OrderDetailStatus.Reserved, false)]
        [TestCase(1, 50, "", null, OrderDetailStatus.Received, false)]
        //[TestCase(1, 50, null, ReservationStatus.Reserved)]
        public async Task Should_Return_With_Condition(
            int pageIndex = 1,
            int pageSize = 50,
            string searchValue = "",
            OrderDetailProperty? orderBy = null,
            OrderDetailStatus? status = null,
            bool IsDescending = false)
        {
            //Arrange
            var request = new GetOrderDetailWithPaginationQuery()
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                SearchValue = searchValue,
                OrderBy = orderBy,
                Status = status,
                IsDescending = IsDescending
            };
            var handler = new GetOrderDetailWithPaginationQueryHandler(_unitOfWork, _mapper, _datetime);
            var conditionedList = _OrderDetails;

            if (!string.IsNullOrWhiteSpace(request.SearchValue))
            {
                conditionedList = conditionedList.Where(e => e.OrderId.Contains(request.SearchValue)
                    || request.SearchValue.Equals(e.FoodId.ToString())
                    || request.SearchValue.Equals(e.Id.ToString())).ToList();
            }
            if (request.Status is not null)
            {
                conditionedList = conditionedList.Where(e => e.Status == request.Status).ToList();
            }

            switch (request.OrderBy)
            {
                case (OrderDetailProperty.OrderId):
                    if (request.IsDescending)
                    {
                        conditionedList = conditionedList.OrderByDescending(x => x.OrderId).ToList();
                        break;
                    }
                    conditionedList = conditionedList.OrderBy(x => x.OrderId).ToList();
                    break;
                case (OrderDetailProperty.FoodId):
                    if (request.IsDescending)
                    {
                        conditionedList = conditionedList.OrderByDescending(x => x.FoodId).ToList();
                        break;
                    }
                    conditionedList = conditionedList.OrderBy(x => x.FoodId).ToList();
                    break;
                case (OrderDetailProperty.Status):
                    if (request.IsDescending)
                    {
                        conditionedList = conditionedList.OrderByDescending(x => x.Status).ToList();
                        break;
                    }
                    conditionedList = conditionedList.OrderBy(x => x.Status).ToList();
                    break;
                case (OrderDetailProperty.Price):
                    if (request.IsDescending)
                    {
                        conditionedList = conditionedList.OrderByDescending(x => x.Price).ToList();
                        break;
                    }
                    conditionedList = conditionedList.OrderBy(x => x.Price).ToList();
                    break;
                default:
                    conditionedList = conditionedList.OrderBy(x => x.Status).ThenBy(e => e.Created).ToList();
                    break;
            }

            conditionedList = conditionedList.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            var expectedList = new List<DishDto>();
            foreach (var dishDto in conditionedList)
            {
                var expectedDto = new DishDto
                {
                    Id = dishDto.Id,
                    UserId = dishDto.Order.UserId,
                    Date = dishDto.Order.Date,
                    OrderId = dishDto.OrderId,
                    Status = dishDto.Status,
                    PhoneNumber = dishDto.Order.User.PhoneNumber,
                    FoodId = dishDto.FoodId,
                    Note = dishDto.Note,
                    FoodName = dishDto.Food.Name,
                    Price = dishDto.Price
                };

                var reservation = _Reservations.Find(e => e.Id == dishDto.Order.ReservationId);
                if (reservation.ReservationTables.Any())
                {
                    expectedDto.TableId = reservation.ReservationTables[0].TableId;
                }

                expectedList.Add(expectedDto);
            }

            var expected = new Response<PaginatedList<DishDto>>(new PaginatedList<DishDto>(expectedList, _OrderDetails.Count, pageIndex, pageSize));
            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.That(actual.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(actual.Data, Is.Not.Null);
            Assert.That(actual, Is.TypeOf(expected.GetType()));
            Assert.That(expected.Data.Count, Is.EqualTo(actual.Data.Count));
            Assert.IsTrue(actual.Data.SequenceEqual(expected.Data, new DishDtoComparer()));
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
            mockOrderDetailRepository
                .Setup(m => m.GetPaginatedListAsync(It.IsAny<List<Expression<Func<OrderDetail, bool>>>>()
                    , It.IsAny<Func<IQueryable<OrderDetail>, IOrderedQueryable<OrderDetail>>>()
                    , It.IsAny<string>()
                    , It.IsAny<int>()
                    , It.IsAny<int>()))
                .ReturnsAsync(
                (List<Expression<Func<OrderDetail, bool>>> filters,
                    Func<IQueryable<OrderDetail>, IOrderedQueryable<OrderDetail>> OrderBy,
                    string includeProperties,
                    int pageIndex,
                    int pageSize)
                =>
                {
                    var query = _OrderDetails.AsQueryable();

                    if (filters is not null)
                    {
                        foreach (var filter in filters)
                        {
                            query = query.Where(filter);
                        }
                    }

                    query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);

                    return OrderBy is not null
                            ? new PaginatedList<OrderDetail>(OrderBy(query).ToList(), query.Count(), pageIndex, pageSize)
                            : new PaginatedList<OrderDetail>(query.ToList(), query.Count(), pageIndex, pageSize);
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
            mapperMock.Setup(m => m.Map<PaginatedList<OrderDetail>, PaginatedList<DishDto>>(It.IsAny<PaginatedList<OrderDetail>>()))
                .Returns((PaginatedList<OrderDetail> entities) =>
                {
                    var dtos = new List<DishDto>();
                    foreach (var entity in entities)
                    {
                        dtos.Add(new DishDto
                        {
                            Id = entity.Id,
                            UserId = entity.Order.UserId,
                            Date = entity.Order.Date,
                            OrderId = entity.OrderId,
                            Status = entity.Status,
                            PhoneNumber = entity.Order.User.PhoneNumber,
                            TableId = 0,
                            FoodId = entity.FoodId,
                            Note = entity.Note,
                            FoodName = entity.Food.Name,
                            Price = entity.Price
                        });
                    }
                    return new PaginatedList<DishDto>(entities.PageNumber, entities.TotalPages, entities.TotalPages, dtos);
                });
            mapperMock.Setup(m => m.Map<DishDto>(It.IsAny<OrderDetail>()))
                .Returns((OrderDetail detail) =>
                    new DishDto
                    {
                        Id = detail.Id,
                        UserId = detail.Order.UserId,
                        Date = detail.Order.Date,
                        OrderId = detail.OrderId,
                        Status = detail.Status,
                        PhoneNumber = detail.Order.User.PhoneNumber,
                        TableId = 0,
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

        private IDateTime SetUpDatetime()
        {
            var dateTimeMock = new Mock<IDateTime>();
            dateTimeMock.Setup(d => d.Now).Returns(DateTime.UtcNow.AddHours(7));
            return dateTimeMock.Object;
        }
        #endregion
    }
}
