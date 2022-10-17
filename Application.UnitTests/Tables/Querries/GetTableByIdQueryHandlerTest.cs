using Application.Common.Exceptions;
using Application.Tables.Queries;
using Application.Tables.Response;
using Application.Models;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;
using System.Net;
using Application.TableTypes.Response;
using Microsoft.EntityFrameworkCore;

namespace Application.UnitTests.Tables.Queries
{
    [TestFixture]
    public class GetTableWithIdQueryHandlerTest
    {
        private List<Table> _tables;
        private List<TableType> _tableTypes;
        private ITableRepository _tableRepository;
        private ITableTypeRepository _tableTypeRepository;
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;

        [SetUp]
        public void ReInitializeTest()
        {
            _tables = DataSource.Tables;
            _tableTypes = DataSource.TableTypes;

            foreach (Table table in _tables)
            {
                table.TableType = _tableTypes.Find(tt => tt.Id == table.TableTypeId);
            }

            _tableRepository = SetUpTableRepository();
            _tableTypeRepository = SetUpTableTypeRepository();
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(x => x.TableRepository).Returns(_tableRepository);
            unitOfWork.SetupGet(x => x.TableTypeRepository).Returns(_tableTypeRepository);
            _unitOfWork = unitOfWork.Object;
            _mapper = SetUpMapper();
        }

        [OneTimeSetUp]
        public void SetUp()
        {
            _tables = DataSource.Tables;
            _tableTypes = DataSource.TableTypes;
            foreach (Table table in _tables)
            {
                table.TableType = _tableTypes.Find(tt => tt.Id == table.TableTypeId);
            }
        }

        [TearDown]
        public void DisposeTest()
        {
            _tableRepository = null;
            _tableTypeRepository = null;
            _unitOfWork = null;
        }

        [OneTimeTearDown]
        public void DisposeAllObjects()
        {
            _tables = null;
            _tableTypes = null;
        }

        #region Unit Tests
        [TestCase(1)]
        public async Task Should_Return_Table(int id)
        {
            //Arrange
            var request = new GetTableWithIdQuery()
            {
                Id = id
            };
            var handler = new GetTableWithIdQueryHandler(_unitOfWork, _mapper);

            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            var inDatabase = _tables.FirstOrDefault(x => x.Id == id);
            var tableType = _tableTypes.Find(tt => tt.Id == inDatabase.TableTypeId);
            var mappedTableType = _mapper.Map<TableTypeDto>(tableType);
            Assert.NotNull(inDatabase);
            var expected = new Response<TableDto>(new TableDto
            {
                Id = inDatabase.Id,
                NumOfSeats = inDatabase.NumOfSeats,
                Status = inDatabase.Status,
                TableType = mappedTableType
            });
            Assert.That(actual.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(expected.Data, Is.EqualTo(actual.Data));
        }

        [TestCase(10)]
        public async Task Should_Return_Throw_NotFound_Exception(int id)
        {
            //Arrange
            var request = new GetTableWithIdQuery()
            {
                Id = id
            };
            var handler = new GetTableWithIdQueryHandler(_unitOfWork, _mapper);

            //Act
            var ex = Assert.ThrowsAsync<NotFoundException>(async () => await handler.Handle(request, CancellationToken.None));

            //Assert
            Assert.That(ex.Message, Is.EqualTo($"Entity {nameof(Table)} (with {request.Id}) was not found."));
        }
        #endregion Unit Tests

        #region Private member methods
        private ITableRepository SetUpTableRepository()
        {
            var mockTableRepository = new Mock<ITableRepository>();
            mockTableRepository.Setup(m => m.GetAsync(It.IsAny<Expression<Func<Table, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(
                (Expression<Func<Table, bool>> filter,
                string includeString)
                =>
                {
                    return _tables.AsQueryable().FirstOrDefault(filter);
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
            mapperMock.Setup(m => m.Map<TableDto>(It.IsAny<Table>()))
                .Returns((Table Table) => new TableDto
                {
                    Id = Table.Id,
                    NumOfSeats = Table.NumOfSeats,
                    Status = Table.Status,
                    TableType = _mapper.Map<TableTypeDto>(Table.TableType)
                });
            mapperMock.Setup(m => m.Map<TableTypeDto>(It.IsAny<TableType>()))
                .Returns((TableType tableType) => new TableTypeDto
                {
                    Id = tableType.Id,
                    Name = tableType.Name,
                    ChargePerSeat = tableType.ChargePerSeat,
                    CanBeCombined = tableType.CanBeCombined
                });
            return mapperMock.Object;
        }
        #endregion

    }
}
