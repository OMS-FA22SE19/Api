using Application.Models;
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

namespace Application.UnitTests.Orders.Queries
{
    [TestFixture]
    public class GetOrderWithPaginationQueryHandlerTest
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

        [TestCase]
        [TestCase(1, 50, "", null, null, false)]
        [TestCase(1, 1, null, null, null, false)]
        [TestCase(2, 1, null, null, null, false)]
        //[TestCase(1, 50, "123", null, null, false)]
        //[TestCase(1, 50, "4", null, null, false)]
        [TestCase(1, 50, "", OrderProperty.Id, null, false)]
        [TestCase(1, 50, "", OrderProperty.Id, null, true)]
        [TestCase(1, 50, "", OrderProperty.UserId, null, false)]
        [TestCase(1, 50, "", OrderProperty.UserId, null, true)]
        [TestCase(1, 50, "", OrderProperty.Status, null, false)]
        [TestCase(1, 50, "", OrderProperty.Status, null, true)]
        [TestCase(1, 50, "", OrderProperty.PrePaid, null, false)]
        [TestCase(1, 50, "", OrderProperty.PrePaid, null, true)]
        [TestCase(1, 50, "", null, OrderStatus.Processing, false)]
        [TestCase(1, 50, "", null, OrderStatus.Reserved, false)]
        [TestCase(1, 50, "", null, OrderStatus.Paid, false)]
        //[TestCase(1, 50, null, ReservationStatus.Reserved)]
        public async Task Should_Return_With_Condition(
            int pageIndex = 1,
            int pageSize = 50,
            string searchValue = "",
            OrderProperty? orderBy = null,
            OrderStatus? status = null,
            bool IsDescending = false)
        {
            //Arrange
            var request = new GetOrderWithPaginationQuery()
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                SearchValue = searchValue,
                OrderBy = orderBy,
                Status = status,
                IsDescending = IsDescending
            };
            var handler = new GetOrderWithPaginationQueryHandler(_unitOfWork, _mapper);
            var conditionedList = _Orders;

            if (!string.IsNullOrWhiteSpace(request.SearchValue))
            {
                conditionedList = conditionedList.Where(e => e.Id.Contains(request.SearchValue)
                    || e.UserId.Contains(request.SearchValue)
                    || e.Date.ToString().Contains(request.SearchValue)).ToList();
            }
            if (request.Status is not null)
            {
                conditionedList = conditionedList.Where(e => e.Status == request.Status).ToList();
            }

            switch (request.OrderBy)
            {
                case (OrderProperty.Id):
                    if (request.IsDescending)
                    {
                        conditionedList = conditionedList.OrderByDescending(x => x.Id).ToList();
                        break;
                    }
                    conditionedList = conditionedList.OrderBy(x => x.Id).ToList();
                    break;
                case (OrderProperty.UserId):
                    if (request.IsDescending)
                    {
                        conditionedList = conditionedList.OrderByDescending(x => x.UserId).ToList();
                        break;
                    }
                    conditionedList = conditionedList.OrderBy(x => x.UserId).ToList();
                    break;
                case (OrderProperty.Status):
                    if (request.IsDescending)
                    {
                        conditionedList = conditionedList.OrderByDescending(x => x.Status).ToList();
                        break;
                    }
                    conditionedList = conditionedList.OrderBy(x => x.Status).ToList();
                    break;
                case (OrderProperty.PrePaid):
                    if (request.IsDescending)
                    {
                        conditionedList = conditionedList.OrderByDescending(x => x.PrePaid).ToList();
                        break;
                    }
                    conditionedList = conditionedList.OrderBy(x => x.PrePaid).ToList();
                    break;
                default:
                    break;
            }

            conditionedList = conditionedList.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            var expectedList = new List<OrderDto>();
            foreach (var orderDto in conditionedList)
            {
                var user = _Users.Find(ft => ft.Id == orderDto.UserId);

                var reservation = _Reservations.Find(r => r.Id == orderDto.ReservationId);
                var expectedDto = new OrderDto
                {
                    Id = orderDto.Id,
                    UserId = orderDto.UserId,
                    Date = orderDto.Date,
                    PrePaid = orderDto.PrePaid,
                    Status = orderDto.Status,
                    NumOfEdits = orderDto.NumOfEdits,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    OrderDetails = new List<OrderDetailDto>()
                };

                double total = 0;
                List<OrderDetailDto> orderDetails = new();
                foreach (var detail in orderDto.OrderDetails)
                {
                    if (detail.Status != OrderDetailStatus.Cancelled)
                    {
                        var element = orderDetails.FirstOrDefault(e => e.FoodId.Equals(detail.FoodId));
                        if (element is null)
                        {
                            orderDetails.Add(new OrderDetailDto
                            {
                                OrderId = orderDto.Id,
                                UserId = orderDto.UserId,
                                Date = orderDto.Date,
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
                total -= orderDto.PrePaid;

                expectedDto.OrderDetails = orderDetails;
                expectedDto.Amount = total - orderDto.PrePaid;
                expectedDto.Total = total;
                if (reservation.ReservationTables.Any())
                {
                    expectedDto.TableId = reservation.ReservationTables[0].TableId;
                }
                expectedList.Add(expectedDto);
            }

            var expected = new Response<PaginatedList<OrderDto>>(new PaginatedList<OrderDto>(expectedList, _Reservations.Count, pageIndex, pageSize));
            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.That(actual.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(actual.Data, Is.Not.Null);
            Assert.That(actual, Is.TypeOf(expected.GetType()));
            Assert.That(expected.Data.Count, Is.EqualTo(actual.Data.Count));
            Assert.IsTrue(actual.Data.SequenceEqual(expected.Data, new OrderDtoComparer()));
        }
        #endregion Unit Tests

        #region Private member methods
        private IOrderRepository SetUpOrderRepository()
        {
            var mockOrderRepository = new Mock<IOrderRepository>();
            mockOrderRepository
                .Setup(m => m.GetPaginatedListAsync(It.IsAny<List<Expression<Func<Order, bool>>>>()
                    , It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>()
                    , It.IsAny<string>()
                    , It.IsAny<int>()
                    , It.IsAny<int>()))
                .ReturnsAsync(
                (List<Expression<Func<Order, bool>>> filters,
                    Func<IQueryable<Order>, IOrderedQueryable<Order>> orderBy,
                    string includeProperties,
                    int pageIndex,
                    int pageSize)
                =>
                {
                    var query = _Orders.AsQueryable();

                    if (filters is not null)
                    {
                        foreach (var filter in filters)
                        {
                            query = query.Where(filter);
                        }
                    }

                    query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);

                    return orderBy is not null
                            ? new PaginatedList<Order>(orderBy(query).ToList(), query.Count(), pageIndex, pageSize)
                            : new PaginatedList<Order>(query.ToList(), query.Count(), pageIndex, pageSize);
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
            mapperMock.Setup(m => m.Map<PaginatedList<Order>, PaginatedList<OrderDto>>(It.IsAny<PaginatedList<Order>>()))
                .Returns((PaginatedList<Order> entities) =>
                {
                    var dtos = new List<OrderDto>();
                    foreach (var entity in entities)
                    {
                        dtos.Add(new OrderDto
                        {
                            Id = entity.Id,
                            UserId = entity.UserId,
                            Date = entity.Date,
                            PrePaid = entity.PrePaid,
                            Status = entity.Status,
                            NumOfEdits = entity.NumOfEdits,
                            FullName = entity.User.FullName,
                            PhoneNumber = entity.User.PhoneNumber
                        });
                    }
                    return new PaginatedList<OrderDto>(entities.PageNumber, entities.TotalPages, entities.TotalPages, dtos);
                });
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
