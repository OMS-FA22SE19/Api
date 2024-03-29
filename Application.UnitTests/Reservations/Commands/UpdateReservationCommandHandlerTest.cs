﻿using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Models;
using Application.Reservations.Commands;
using Application.Reservations.Response;
using AutoMapper;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using Firebase.Auth;
using FluentAssertions.Common;
using Microsoft.AspNetCore.Identity;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;
using System.Net;

namespace Application.UnitTests.Reservations.Commands
{
    public class UpdateReservationCommandHandlerTest
    {
        private List<Reservation> _Reservations;
        private List<ReservationTable> _ReservationTables;
        private List<Table> _Tables;
        private List<TableType> _TableTypes;
        private List<Billing> _Billing;
        private List<ApplicationUser> _Users;
        private UserManager<ApplicationUser> _UserManager;
        private IReservationRepository _ReservationRepository;
        private ITableRepository _TableRepository;
        private ITableTypeRepository _TableTypeRepository;
        private IReservationTableRepository _ReservationTableRepository;
        private IBillingRepository _BillingRepository;
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;
        private ICurrentUserService _currentUserService;

        [SetUp]
        public void ReInitializeTest()
        {
            _Reservations = DataSource.Reservations;
            _ReservationTables = DataSource.ReservationTables;
            _Tables = DataSource.Tables;
            _TableTypes = DataSource.TableTypes;
            _Billing = DataSource.Billings;
            _Users = DataSource.Users;
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
            _UserManager = MockUserManager<ApplicationUser>(_Users).Object;
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
            _Reservations = null;
            _ReservationTables = null;
            _Tables = null;
            _TableTypes = null;
            _Billing = null;
            _Users = null;
        }

        #region Unit Tests
        // TODO: [TestCase(1, 4, 2, 2, 1)]
        // TODO: [TestCase(3, 4, 2, 2, 1)]
        public async Task Should_Update_Reservation(int id, int numOfSeat, int numOfPeople, int tableTypeId, int quantity)
        {
            //Arrange
            DateTime startTime = DateTime.UtcNow;
            DateTime endTime = startTime.AddHours(1);
            var request = new UpdateReservationCommand()
            {
                Id = id,
                NumOfSeats = numOfSeat,
                StartTime = startTime,
                EndTime = endTime,
                NumOfPeople = numOfPeople,
                Quantity = quantity,
                TableTypeId = tableTypeId
            };
            var handler = new UpdateReservationCommandHandler(_unitOfWork, _mapper, _UserManager, _currentUserService);
            var reservation = _Reservations.FirstOrDefault(e => e.Id == id);
            var billing = _Billing.FirstOrDefault(b => b.ReservationId == reservation.Id);
            
            var expected = new Reservation()
            {
                Id = id,
                UserId = "123",
                StartTime = startTime,
                EndTime = endTime,
                NumOfPeople = numOfPeople,
                NumOfSeats = numOfSeat,
                TableTypeId = tableTypeId,
                Quantity = quantity,
                Status = reservation.Status,
                IsPriorFoodOrder = false,

            };
            if (billing is not null)
            {
                var type = _TableTypes.Find(t => t.Id == tableTypeId);
                if (billing.ReservationAmount <= numOfSeat * quantity * type.ChargePerSeat)
                {
                    expected.Status = ReservationStatus.Available;
                }
            }
            
            //Act
            var actual = await handler.Handle(request, CancellationToken.None);
            

            //Assert
            Assert.That(actual.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(actual, Is.TypeOf(typeof(Response<ReservationDto>)));
            Assert.Null(actual.Data);
            var inDatabase = _Reservations.FirstOrDefault(e => e.Id == id);
            Assert.That(inDatabase, Is.EqualTo(expected));
        }

        // TODO: [TestCase(1, 4, 2, 10, 1)]
        // TODO: [TestCase(1, 4, 2, 0, 1)]
        public async Task Should_Return_Throw_NotFound_ReservationId_Exception(int id, int numOfSeat, int numOfPeople, int tableTypeId, int quantity)
        {
            //Arrange
            var request = new UpdateReservationCommand()
            {
                Id = id,
                NumOfSeats = numOfSeat,
                NumOfPeople = numOfPeople,
                Quantity = quantity,
                TableTypeId = tableTypeId
            };
            var handler = new UpdateReservationCommandHandler(_unitOfWork, _mapper, _UserManager, _currentUserService);

            //Act
            var ex = Assert.ThrowsAsync<NotFoundException>(async () => await handler.Handle(request, CancellationToken.None));

            //Assert
            Assert.That(ex.Message, Is.EqualTo($"Entity {nameof(TableType)} ({request.TableTypeId}) was not found."));
        }

        [TestCase(10)]
        [TestCase(0)]
        public async Task Should_Return_Throw_NotFound_ReservationTypeId_Exception(int id)
        {
            //Arrange
            var request = new UpdateReservationCommand()
            {
                Id = id
            };
            var handler = new UpdateReservationCommandHandler(_unitOfWork, _mapper, _UserManager, _currentUserService);

            //Act
            var ex = Assert.ThrowsAsync<NotFoundException>(async () => await handler.Handle(request, CancellationToken.None));

            //Assert
            Assert.That(ex.Message, Is.EqualTo($"Entity {nameof(Reservation)} ({request.Id}) was not found."));
        }
        #endregion Unit Tests

        #region Private member methods
        private IReservationRepository SetUpReservationRepository()
        {
            var mockReservationRepository = new Mock<IReservationRepository>();
            mockReservationRepository.Setup(m => m.GetAsync(It.IsAny<Expression<Func<Reservation, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(
                (Expression<Func<Reservation, bool>> expression,
                string includeProperties)
                =>
                {
                    return _Reservations.AsQueryable().FirstOrDefault(expression);
                });
            mockReservationRepository.Setup(m => m.UpdateAsync(It.IsAny<Reservation>()))
                .ReturnsAsync(
                (Reservation updatedEntity)
                =>
                {
                    var inDatabase = _Reservations.FirstOrDefault(e => e.Id == updatedEntity.Id);
                    inDatabase.StartTime = updatedEntity.StartTime;
                    inDatabase.EndTime = updatedEntity.EndTime;
                    inDatabase.NumOfPeople = updatedEntity.NumOfPeople;
                    inDatabase.NumOfSeats = updatedEntity.NumOfSeats;
                    inDatabase.TableTypeId = updatedEntity.TableTypeId;
                    inDatabase.Quantity = updatedEntity.Quantity;
                    return inDatabase;
                });
            mockReservationRepository.Setup(m => m.GetAllReservationWithDate(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(
                (DateTime dateTime, int? tableTypeId, int? numOfSeat) =>
                {
                    var list = _Reservations.Where(r =>
                        r.StartTime.Date == dateTime.Date
                        && r.EndTime.Date == dateTime.Date
                        && r.Status != ReservationStatus.Available
                        && r.Status != ReservationStatus.Cancelled
                        && r.Status != ReservationStatus.Done
                        && !r.IsDeleted).OrderBy(r => r.StartTime).ToList();

                    if (tableTypeId is not null)
                    {
                        list = list.Where(e => e.TableTypeId == tableTypeId).ToList();
                        if (numOfSeat is not null)
                        {
                            list = list.Where(e => e.NumOfSeats == numOfSeat).ToList();
                        }
                    }

                    return list.ToList();
                });
            return mockReservationRepository.Object;
        }

        public static Mock<UserManager<TUser>> MockUserManager<TUser>(List<TUser> ls) where TUser : class
        {
            var store = new Mock<IUserStore<TUser>>();
            var mgr = new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
            mgr.Object.UserValidators.Add(new UserValidator<TUser>());
            mgr.Object.PasswordValidators.Add(new PasswordValidator<TUser>());

            mgr.Setup(x => x.DeleteAsync(It.IsAny<TUser>())).ReturnsAsync(IdentityResult.Success);
            mgr.Setup(x => x.CreateAsync(It.IsAny<TUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success).Callback<TUser, string>((x, y) => ls.Add(x));
            mgr.Setup(x => x.UpdateAsync(It.IsAny<TUser>())).ReturnsAsync(IdentityResult.Success);
            //mgr.Setup(x => x.Users).Returns(_Users);
            var mock = ls.AsQueryable().BuildMock();

            //3 - setup the mock as Queryable for Moq
            mgr.Setup(x => x.Users).Returns(ls.AsQueryable().BuildMock());

            //3 - setup the mock as Queryable for NSubstitute
            //mgr.GetQueryable().Returns(mock);

            return mgr;
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

            mockTableRepository.Setup(m => m.GetTableOnNumOfSeatAndType(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(
                (int? NumOfSeats, int? tableTypeId) =>
                {
                    var list = _Tables.Where(t => t.NumOfSeats == NumOfSeats && !t.IsDeleted && t.TableTypeId == tableTypeId).ToList();
                    //string includeStrings = $"{nameof(Table.TableType)}";
                    //_Tables = _Tables.Where(t => t.NumOfSeats == NumOfSeats && !t.IsDeleted && t.TableTypeId == tableTypeId).ToList();

                    return list.ToList();
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
            mapperMock.Setup(m => m.Map<Reservation>(It.IsAny<UpdateReservationCommand>()))
                .Returns((UpdateReservationCommand command) => new Reservation
                {
                    NumOfSeats = command.NumOfSeats,
                    Status = ReservationStatus.Available,
                    NumOfPeople = command.NumOfPeople,
                    Quantity = command.Quantity,
                    StartTime = command.StartTime,
                    EndTime = command.EndTime,
                    TableTypeId = command.TableTypeId
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
