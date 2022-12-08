using Application.Common.Interfaces;
using Application.Models;
using Application.Reservations.Queries;
using Application.Reservations.Response;
using Application.Tables.Response;
using AutoMapper;
using Core.Common;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using Infrastructure.Repositories;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;
using System.Net;
using System.Web.Mvc;

namespace Application.UnitTests.Reservations.Queries
{
    [TestFixture]
    public class GetReservationWithPaginationQueryHandlerTest
    {
        private List<Reservation> _Reservations;
        private List<ReservationTable> _ReservationTables;
        private List<Table> _Tables;
        private List<TableType> _TableTypes;
        private List<Billing> _Billing;
        private List<Order> _Orders;
        private List<OrderDetail> _OrderDetails;
        private List<Food> _Foods;

        private IReservationRepository _ReservationRepository;
        private ITableRepository _TableRepository;
        private ITableTypeRepository _TableTypeRepository;
        private IReservationTableRepository _ReservationTableRepository;
        private IBillingRepository _BillingRepository;
        private IOrderRepository _OrderRepository;
        private IOrderDetailRepository _OrderDetailRepository;
        private IFoodRepository _FoodRepository;
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;
        private ICurrentUserService _currentUserService;

        [OneTimeSetUp]
        public void SetUp()
        {
            _Reservations = DataSource.Reservations;
            _ReservationTables = DataSource.ReservationTables;
            _Tables = DataSource.Tables;
            _TableTypes = DataSource.TableTypes;
            _Billing = DataSource.Billings;
            _Orders = DataSource.Orders;
            _OrderDetails = DataSource.OrderDetails;
            _Foods = DataSource.Foods;
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
            foreach (Order order in _Orders)
            {
                order.OrderDetails = _OrderDetails.FindAll(ft => ft.OrderId == order.Id);
                if (order.OrderDetails != null)
                {
                    foreach (OrderDetail detail in order.OrderDetails)
                    {
                        detail.Food = _Foods.Find(t => t.Id == detail.FoodId);
                    }
                }
            }
        }

        [SetUp]
        public void ReInitializeTest()
        {
            _Reservations = DataSource.Reservations;
            _ReservationTables = DataSource.ReservationTables;
            _Tables = DataSource.Tables;
            _TableTypes = DataSource.TableTypes;
            _Billing = DataSource.Billings;
            _Orders = DataSource.Orders;
            _OrderDetails = DataSource.OrderDetails;
            _Foods = DataSource.Foods;
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
            foreach (Order order in _Orders)
            {
                order.OrderDetails = _OrderDetails.FindAll(ft => ft.OrderId == order.Id);
                if (order.OrderDetails != null)
                {
                    foreach (OrderDetail detail in order.OrderDetails)
                    {
                        detail.Food = _Foods.Find(t => t.Id == detail.FoodId);
                    }
                }
            }
            _ReservationRepository = SetUpReservationRepository();
            _TableRepository = SetUpTableRepository();
            _ReservationTableRepository = SetUpReservationTableRepository();
            _TableTypeRepository = SetUpTableTypeRepository();
            _BillingRepository = SetUpBillingRepository();
            _OrderRepository = SetUpOrderRepository();
            _OrderDetailRepository = SetUpOrderDetailRepository();
            _FoodRepository = SetUpFoodRepository();
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(x => x.ReservationRepository).Returns(_ReservationRepository);
            unitOfWork.SetupGet(x => x.ReservationTableRepository).Returns(_ReservationTableRepository);
            unitOfWork.SetupGet(x => x.TableRepository).Returns(_TableRepository);
            unitOfWork.SetupGet(x => x.TableTypeRepository).Returns(_TableTypeRepository);
            unitOfWork.SetupGet(x => x.BillingRepository).Returns(_BillingRepository);
            unitOfWork.SetupGet(x => x.OrderRepository).Returns(_OrderRepository);
            unitOfWork.SetupGet(x => x.OrderDetailRepository).Returns(_OrderDetailRepository);
            unitOfWork.SetupGet(x => x.FoodRepository).Returns(_FoodRepository);
            _unitOfWork = unitOfWork.Object;
            _mapper = SetUpMapper();
            _currentUserService = SetCurrentUserService();
        }

        [TearDown]
        public void DisposeTest()
        {
            _ReservationRepository = null;
            _TableRepository = null;
            _ReservationTableRepository = null;
            _TableTypeRepository = null;
            _BillingRepository = null;
            _OrderRepository = null;
            _OrderDetailRepository = null;
            _unitOfWork = null;
        }

        [OneTimeTearDown]
        public void DisposeAllObjects()
        {
            _Reservations = null;
            _ReservationTables = null;
            _TableTypes = null;
            _Tables = null;
            _Billing = null;
            _Orders = null;
            _OrderDetails = null;
            _Foods = null;
        }

        #region Unit Tests

        //TODO:[TestCase]
        //TODO:[TestCase(1, 50, null, null)]
        //TODO:[TestCase(1, 2, null, null)]
        //TODO:[TestCase(2, 2, null, null)]
        //TODO:[TestCase(1, 50, "123", null)]
        //TODO: [TestCase(1, 50, "4", null)]
        //TODO:[TestCase(1, 50, null, ReservationStatus.Available)]
        //TODO:[TestCase(1, 50, null, ReservationStatus.CheckIn)]
        //TODO:[TestCase(1, 50, null, ReservationStatus.Cancelled)]
        //TODO:[TestCase(1, 50, null, ReservationStatus.Reserved)]
        public async Task Should_Return_With_Condition(
            int pageIndex = 1,
            int pageSize = 50,
            string? userId = null,
            ReservationStatus? status = null)
        {
            //Arrange
            var request = new GetReservationWithPaginationQuery()
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                userId= userId,
                Status= status
            };
            var handler = new GetReservationWithPaginationQueryHandler(_unitOfWork, _mapper, _currentUserService);
            var conditionedList = _Reservations;

            if (!string.IsNullOrWhiteSpace(request.userId))
            {
                conditionedList = conditionedList.Where(e => e.UserId.Contains(request.userId)).ToList();
            }
            if (request.Status is not null)
            {
                conditionedList = conditionedList.Where(e => e.Status == request.Status).ToList();
            }

            conditionedList = conditionedList.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            var expectedList = new List<ReservationDto>();
            foreach (var reservationTypeDto in conditionedList)
            {
                var mappedTable = new List<ReservationTableDto>();

                var ReservationTables = _ReservationTables.FindAll(ft => ft.ReservationId == reservationTypeDto.Id);
                if (ReservationTables.Any())
                {
                    foreach (ReservationTable reservationTable in ReservationTables)
                    {
                        var table = _Tables.Find(t => t.Id == reservationTable.TableId);
                        reservationTable.Table = table;
                        mappedTable.Add(_mapper.Map<ReservationTableDto>(reservationTable));
                    }
                }

                var type = _TableTypes.Find(t => t.Id == reservationTypeDto.TableTypeId);
                var prepaid = type.ChargePerSeat * reservationTypeDto.NumOfSeats * reservationTypeDto.Quantity;
                var billing = _Billing.FirstOrDefault(b => b.ReservationId == reservationTypeDto.Id);
                double paid = 0;
                if (billing is not null)
                {
                    paid = billing.ReservationAmount;
                }
                expectedList.Add(new ReservationDto
                {
                    Id = reservationTypeDto.Id,
                    UserId = reservationTypeDto.UserId,
                    StartTime = reservationTypeDto.StartTime,
                    EndTime = reservationTypeDto.EndTime,
                    NumOfPeople = reservationTypeDto.NumOfPeople,
                    NumOfSeats = reservationTypeDto.NumOfSeats,
                    TableTypeId = reservationTypeDto.TableTypeId,
                    Quantity = reservationTypeDto.Quantity,
                    Status = reservationTypeDto.Status,
                    IsPriorFoodOrder = reservationTypeDto.IsPriorFoodOrder,
                    TableType = type.Name,
                    PrePaid = prepaid,
                    Paid= paid,
                });
            }

            var expected = new Response<PaginatedList<ReservationDto>>(new PaginatedList<ReservationDto>(expectedList, _Reservations.Count, pageIndex, pageSize));
            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.That(actual.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(actual.Data, Is.Not.Null);
            Assert.That(actual, Is.TypeOf(expected.GetType()));
            Assert.That(expected.Data.Count, Is.EqualTo(actual.Data.Count));
            Assert.IsTrue(actual.Data.SequenceEqual(expected.Data, new ReservationDtoComparer()));
        }
        #endregion Unit Tests

        #region Private member methods
        private IReservationRepository SetUpReservationRepository()
        {
            var mockReservationRepository = new Mock<IReservationRepository>();
            mockReservationRepository
                .Setup(m => m.GetPaginatedListAsync(It.IsAny<List<Expression<Func<Reservation, bool>>>>()
                    , It.IsAny<Func<IQueryable<Reservation>, IOrderedQueryable<Reservation>>>()
                    , It.IsAny<string>()
                    , It.IsAny<int>()
                    , It.IsAny<int>()))
                .ReturnsAsync(
                (List<Expression<Func<Reservation, bool>>> filters,
                    Func<IQueryable<Reservation>, IOrderedQueryable<Reservation>> orderBy,
                    string includeProperties,
                    int pageIndex,
                    int pageSize)
                =>
                {
                    var query = _Reservations.AsQueryable();

                    if (filters is not null)
                    {
                        foreach (var filter in filters)
                        {
                            query = query.Where(filter);
                        }
                    }

                    query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);

                    return orderBy is not null
                            ? new PaginatedList<Reservation>(orderBy(query).ToList(), query.Count(), pageIndex, pageSize)
                            : new PaginatedList<Reservation>(query.ToList(), query.Count(), pageIndex, pageSize);
                });
            return mockReservationRepository.Object;
        }

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

        private IMapper SetUpMapper()
        {
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<PaginatedList<Reservation>, PaginatedList<ReservationDto>>(It.IsAny<PaginatedList<Reservation>>()))
                .Returns((PaginatedList<Reservation> entities) =>
                {
                    var dtos = new List<ReservationDto>();
                    foreach (var entity in entities)
                    {
                        dtos.Add(new ReservationDto
                        {
                            Id = entity.Id,
                            UserId = entity.UserId,
                            StartTime = entity .StartTime,
                            EndTime = entity.EndTime,
                            NumOfPeople = entity.NumOfPeople,
                            NumOfSeats = entity.NumOfSeats,
                            TableTypeId = entity.TableTypeId,
                            Quantity = entity.Quantity,
                            Status = entity.Status,
                            IsPriorFoodOrder = entity.IsPriorFoodOrder
                        });
                    }
                    return new PaginatedList<ReservationDto>(entities.PageNumber, entities.TotalPages, entities.TotalPages, dtos);
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

        private ICurrentUserService SetCurrentUserService()
        {
            var currentUserMock = new Mock<ICurrentUserService>();
            return currentUserMock.Object;
        }
        #endregion
    }
}
