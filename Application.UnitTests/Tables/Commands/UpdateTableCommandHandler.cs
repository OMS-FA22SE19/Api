using Application.Common.Exceptions;
using Application.Models;
using Application.Tables.Commands;
using Application.Tables.Response;
using AutoMapper;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;
using System.Net;

namespace Application.UnitTests.Tables.Commands
{
    public class UpdateTableCommandHandlerTest
    {
        private List<Table> _tables;
        private List<TableType> _tableTypes;
        private ITableRepository _tableRepository;
        private ITableTypeRepository _TableTypeRepository;
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;

        [SetUp]
        public void ReInitializeTest()
        {
            _tables = DataSource.Tables;
            _tableTypes = DataSource.TableTypes;
            _tableRepository = SetUpTableRepository();
            _TableTypeRepository = SetUpTableTypeRepository();
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(x => x.TableRepository).Returns(_tableRepository);
            unitOfWork.SetupGet(x => x.TableTypeRepository).Returns(_TableTypeRepository);
            _unitOfWork = unitOfWork.Object;
            _mapper = SetUpMapper();
        }

        [TearDown]
        public void DisposeTest()
        {
            _tableRepository = null;
            _TableTypeRepository = null;
            _unitOfWork = null;
            _tables = null;
        }

        #region Unit Tests
        [TestCase(2, 10, TableStatus.Available, 1)]
        public async Task Should_Update_Table(int id, int numOfSeat, TableStatus status, int tableTypeId)
        {
            //Arrange
            var request = new UpdateTableCommand()
            {
                Id = id,
                NumOfSeats = numOfSeat,
                Status = status,
                TableTypeId = tableTypeId
            };
            var handler = new UpdateTableCommandHandler(_unitOfWork, _mapper);
            var expected = new Table()
            {
                Id = id,
                NumOfSeats = numOfSeat,
                Status = status,
                TableTypeId = tableTypeId
            };
            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.That(actual.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(actual, Is.TypeOf(typeof(Response<TableDto>)));
            Assert.Null(actual.Data);
            var inDatabase = _tables.FirstOrDefault(e => e.Id == id);
            Assert.That(inDatabase, Is.EqualTo(expected));
        }
        [TestCase(2, 10)]
        public async Task Should_Return_Throw_NotFound_TableId_Exception(int id, int tableTypeId)
        {
            //Arrange
            var request = new UpdateTableCommand()
            {
                Id = id,
                TableTypeId = tableTypeId
            };
            var handler = new UpdateTableCommandHandler(_unitOfWork, _mapper);

            //Act
            var ex = Assert.ThrowsAsync<NotFoundException>(async () => await handler.Handle(request, CancellationToken.None));

            //Assert
            Assert.That(ex.Message, Is.EqualTo($"Entity {nameof(TableType)} ({request.TableTypeId}) was not found."));
        }

        [TestCase(10)]
        public async Task Should_Return_Throw_NotFound_TableTypeId_Exception(int id)
        {
            //Arrange
            var request = new UpdateTableCommand()
            {
                Id = id
            };
            var handler = new UpdateTableCommandHandler(_unitOfWork, _mapper);

            //Act
            var ex = Assert.ThrowsAsync<NotFoundException>(async () => await handler.Handle(request, CancellationToken.None));

            //Assert
            Assert.That(ex.Message, Is.EqualTo($"Entity {nameof(Table)} ({request.Id}) was not found."));
        }
        #endregion Unit Tests

        #region Private member methods
        private ITableRepository SetUpTableRepository()
        {
            var mockTableRepository = new Mock<ITableRepository>();
            mockTableRepository.Setup(m => m.GetAsync(It.IsAny<Expression<Func<Table, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(
                (Expression<Func<Table, bool>> expression,
                string includeProperties)
                =>
                {
                    return _tables.AsQueryable().FirstOrDefault(expression);
                });
            mockTableRepository.Setup(m => m.UpdateAsync(It.IsAny<Table>()))
                .ReturnsAsync(
                (Table updatedEntity)
                =>
                {
                    var inDatabase = _tables.FirstOrDefault(e => e.Id == updatedEntity.Id);
                    inDatabase.NumOfSeats = updatedEntity.NumOfSeats;
                    inDatabase.Status = updatedEntity.Status;
                    inDatabase.TableTypeId = updatedEntity.TableTypeId;
                    return inDatabase;
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
                    return _tableTypes.AsQueryable().FirstOrDefault(filter);
                });
            return mockTableTypeRepository.Object;
        }

        private IMapper SetUpMapper()
        {
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<Table>(It.IsAny<UpdateTableCommand>()))
                .Returns((UpdateTableCommand command) => new Table
                {
                    Id = command.Id,
                    NumOfSeats = command.NumOfSeats,
                    Status = command.Status,
                    TableTypeId = command.TableTypeId
                });
            return mapperMock.Object;
        }
        #endregion
    }
}
