using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Tables.Commands;
using Application.Tables.Response;
using Application.TableTypes.Response;
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
    [TestFixture]
    public class CreateTableCommandHandlerTest
    {
        private List<Table> _tables;
        private List<TableType> _tableTypes;
        private ITableRepository _TableRepository;
        private ITableTypeRepository _TableTypeRepository;
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;

        [SetUp]
        public void ReInitializeTest()
        {
            _tables = DataSource.Tables;
            _tableTypes = DataSource.TableTypes;
            _TableRepository = SetUpTableRepository();
            _TableTypeRepository = SetUpTableTypeRepository();
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(x => x.TableRepository).Returns(_TableRepository);
            unitOfWork.SetupGet(x => x.TableTypeRepository).Returns(_TableTypeRepository);
            _unitOfWork = unitOfWork.Object;
            _mapper = SetUpMapper();
        }

        [TearDown]
        public void DisposeTest()
        {
            _TableRepository = null;
            _TableTypeRepository = null;
            _unitOfWork = null;
            _tables = null;
        }

        #region Unit Tests
        [TestCase(4, 1)]
        public async Task Should_Create_Table(int numOfSeat, int tableTypeId)
        {
            //Arrange
            var request = new CreateTableCommand()
            {
                NumOfSeats = numOfSeat,
                TableTypeId = tableTypeId
            };
            var handler = new CreateTableCommandHandler(_unitOfWork, _mapper);
            var tableType = _tableTypes.Find(tt => tt.Id == tableTypeId);
            var mappedTableType = _mapper.Map<TableTypeDto>(tableType);
            var expected = new Response<TableDto>(new TableDto
            {
                Id = _tables.Max(e => e.Id) + 1,
                NumOfSeats = numOfSeat,
                Status = TableStatus.Available,
                TableType = mappedTableType
            })
            {
                StatusCode = HttpStatusCode.Created
            };
            var count = _tables.Count + 1;
            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.That(actual.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(_tables.Count, Is.EqualTo(count));
            var inDatabase = _tables.FirstOrDefault(e => e.NumOfSeats == numOfSeat);
            Assert.NotNull(inDatabase);
            Assert.That(actual.Data, Is.EqualTo(expected.Data));
        }

        [TestCase(4, 10)]
        public async Task Should_Return_Throw_NotFound_Exception(int numOfSeat, int tableTypeId)
        {
            //Arrange
            var request = new CreateTableCommand()
            {
                NumOfSeats = numOfSeat,
                TableTypeId = tableTypeId
            };
            var handler = new CreateTableCommandHandler(_unitOfWork, _mapper);

            //Act
            var ex = Assert.ThrowsAsync<NotFoundException>(async () => await handler.Handle(request, CancellationToken.None));

            //Assert
            Assert.That(ex.Message, Is.EqualTo($"Entity {nameof(TableType)} ({request.TableTypeId}) was not found."));
        }
        #endregion Unit Tests

        #region Private member methods
        private ITableRepository SetUpTableRepository()
        {
            var mockTableRepository = new Mock<ITableRepository>();
            mockTableRepository.Setup(m => m.InsertAsync(It.IsAny<Table>()))
                .ReturnsAsync(
                (Table Table)
                =>
                {
                    Table.Id = _tables.Max(e => e.Id) + 1;
                    _tables.Add(Table);
                    return Table;
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
            mapperMock.Setup(m => m.Map<Table>(It.IsAny<CreateTableCommand>()))
                .Returns((CreateTableCommand command) => new Table
                {
                    NumOfSeats = command.NumOfSeats,
                    Status = TableStatus.Available,
                    TableTypeId = command.TableTypeId
                });
            mapperMock.Setup(m => m.Map<TableDto>(It.IsAny<Table>()))
                .Returns((Table Table) => new TableDto
                {
                    Id = Table.Id,
                    NumOfSeats = Table.NumOfSeats,
                    Status = Table.Status,
                    TableType = _mapper.Map<TableTypeDto>(Table.TableType)
                });
            return mapperMock.Object;
        }
        #endregion

    }
}
