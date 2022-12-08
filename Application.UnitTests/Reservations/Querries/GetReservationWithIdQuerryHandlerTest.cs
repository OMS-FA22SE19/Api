using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Reservations.Commands;
using Application.Reservations.Queries;
using Application.Reservations.Response;
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

namespace Application.UnitTests.Reservations.Queries
{
    [TestFixture]
    public class GetReservationWithIdQueryHandlerTest
    {
        private List<Reservation> _Reservations;
        private List<ReservationTable> _ReservationTables;
        private List<Table> _Tables;
        private List<TableType> _TableTypes;
        private List<Billing> _Billing;
        private IReservationRepository _ReservationRepository;
        private ITableRepository _TableRepository;
        private ITableTypeRepository _TableTypeRepository;
        private IReservationTableRepository _ReservationTableRepository;
        private IBillingRepository _BillingRepository;
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
            _Reservations = DataSource.Reservations;
            _ReservationTables = DataSource.ReservationTables;
            _Tables = DataSource.Tables;
            _TableTypes = DataSource.TableTypes;
            _Billing = DataSource.Billings;
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
            _ReservationRepository = SetUpReservationRepository();
            _TableRepository = SetUpTableRepository();
            _ReservationTableRepository = SetUpReservationTableRepository();
            _TableTypeRepository = SetUpTableTypeRepository();
            _BillingRepository = SetUpBillingRepository();
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(x => x.ReservationRepository).Returns(_ReservationRepository);
            unitOfWork.SetupGet(x => x.ReservationTableRepository).Returns(_ReservationTableRepository);
            unitOfWork.SetupGet(x => x.TableRepository).Returns(_TableRepository);
            unitOfWork.SetupGet(x => x.TableTypeRepository).Returns(_TableTypeRepository);
            unitOfWork.SetupGet(x => x.BillingRepository).Returns(_BillingRepository);
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
            _ReservationTableRepository = null;
            _unitOfWork = null;
        }

        [OneTimeTearDown]
        public void DisposeAllObjects()
        {
            _Reservations = null;
            _ReservationTables = null;
            _TableTypes = null;
            _Tables = null;
        }

        #region Unit Tests
        //TODO: [TestCase(3)]
        //TODO: [TestCase(4)]
        public async Task Should_Return_Reservation(int id)
        {
            //Arrange
            var request = new GetReservationWithIdQuery()
            {
                Id = id
            };
            var handler = new GetReservationWithIdQueryHandler(_unitOfWork, _mapper, _currentUserService);

            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            var inDatabase = _Reservations.FirstOrDefault(x => x.Id == id);
            Assert.NotNull(inDatabase);
            var mappedTable = new List<ReservationTableDto>();

            var ReservationTables = _ReservationTables.FindAll(ft => ft.ReservationId == id);
            if (ReservationTables.Any())
            {
                foreach (ReservationTable reservationTable in ReservationTables)
                {
                    var table = _Tables.Find(t => t.Id == reservationTable.TableId);
                    reservationTable.Table = table;
                    mappedTable.Add(_mapper.Map<ReservationTableDto>(reservationTable));
                }
            }

            var type = _TableTypes.Find(t => t.Id == inDatabase.TableTypeId);
            var prepaid = type.ChargePerSeat * inDatabase.NumOfSeats * inDatabase.Quantity;
            var billing = _Billing.FirstOrDefault(b => b.ReservationId == inDatabase.Id);
            var expected = new Response<ReservationDto>(new ReservationDto
            {
                Id = inDatabase.Id,
                UserId = inDatabase.UserId,
                StartTime = inDatabase.StartTime,
                EndTime = inDatabase.EndTime,
                NumOfPeople = inDatabase.NumOfPeople,
                NumOfSeats = inDatabase.NumOfSeats,
                TableTypeId = inDatabase.TableTypeId,
                Quantity = inDatabase.Quantity,
                Status = inDatabase.Status,
                IsPriorFoodOrder = inDatabase.IsPriorFoodOrder,
                TableType = type.Name,
                PrePaid = prepaid
            });
            if (billing is not null)
            {
                expected.Data.Paid = billing.ReservationAmount;
            }

            Assert.That(actual.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(expected.Data, Is.EqualTo(actual.Data));
        }

        [TestCase(10)]
        public async Task Should_Return_Throw_NotFound_Exception(int id)
        {
            //Arrange
            var request = new GetReservationWithIdQuery()
            {
                Id = id
            };
            var handler = new GetReservationWithIdQueryHandler(_unitOfWork, _mapper, _currentUserService);

            //Act
            var ex = Assert.ThrowsAsync<NotFoundException>(async () => await handler.Handle(request, CancellationToken.None));

            //Assert
            Assert.That(ex.Message, Is.EqualTo($"Entity {nameof(Reservation)} (with {request.Id}) was not found."));
        }
        #endregion Unit Tests

        #region Private member methods
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

        private IMapper SetUpMapper()
        {
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<ReservationDto>(It.IsAny<Reservation>()))
                .Returns((Reservation Reservation) => new ReservationDto
                {
                    Id = Reservation.Id,
                    UserId = Reservation.UserId,
                    StartTime = Reservation.StartTime,
                    EndTime = Reservation.EndTime,
                    NumOfPeople = Reservation.NumOfPeople,
                    NumOfSeats = Reservation.NumOfSeats,
                    TableTypeId = Reservation.TableTypeId,
                    Quantity = Reservation.Quantity,
                    Status = Reservation.Status,
                    IsPriorFoodOrder = Reservation.IsPriorFoodOrder
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
