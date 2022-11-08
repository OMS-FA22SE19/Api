using Application.Models;
using Application.Tables.Queries;
using Application.Tables.Response;
using Application.TableTypes.Response;
using AutoMapper;
using Core.Common;
using Core.Entities;
using Core.Interfaces;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;
using System.Net;

namespace Application.UnitTests.Tables.Queries
{
    [TestFixture]
    public class GetTypeOfTableQueryHandlerTest
    {
        private List<Table> _tables;
        private List<TableType> _tableTypes;
        private ITableRepository _tableRepository;
        private ITableTypeRepository _tableTypeRepository;
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;

        [OneTimeSetUp]
        public void SetUp()
        {
            _tables = DataSource.Tables;
        }

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

        [TearDown]
        public void DisposeTest()
        {
            _tableRepository = null;
            _unitOfWork = null;
        }

        [OneTimeTearDown]
        public void DisposeAllObjects()
        {
            _tables = null;
        }

        #region Unit Tests

        [TestCase(6)]
        public async Task Should_Return_With_Condition(int NumsOfPeople)
        {
            //Arrange
            var request = new GetTypeOfTableQuery()
            {
                NumsOfPeople = NumsOfPeople
            };
            var handler = new GetTypeOfTableQueryHandler(_unitOfWork, _mapper);

            var ListTableType = new List<TableByTypeDto>();

            foreach (var table in _tables)
            {
                var tableType = ListTableType.FirstOrDefault(t => t.TableTypeId == table.TableTypeId && t.NumOfSeats == table.NumOfSeats);
                if (tableType == null)
                {
                    int quantity = 1;
                    bool isValid = false;

                    if (table.NumOfSeats >= NumsOfPeople)
                    {
                        isValid = true;
                    }
                    else
                    {
                        var count = _tables.Where(e => e.TableTypeId == table.TableTypeId && e.NumOfSeats == table.NumOfSeats && e.TableType.CanBeCombined).Count();
                        quantity = ((NumsOfPeople % table.NumOfSeats) == 0) ? NumsOfPeople / table.NumOfSeats : (NumsOfPeople / table.NumOfSeats) + 1;
                        isValid = count >= quantity && quantity <= 4;
                    }
                    if (isValid)
                    {
                        ListTableType.Add(new TableByTypeDto()
                        {
                            TableTypeId = table.TableTypeId,
                            TableTypeName = table.TableType.Name,
                            NumOfSeats = table.NumOfSeats,
                            Quantity = quantity
                        });
                    }
                }
            }
            var expected = new Response<List<TableByTypeDto>>(new List<TableByTypeDto>(ListTableType.OrderByDescending(e => e.NumOfSeats).ToList()));


            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.That(actual.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(actual.Data, Is.Not.Null);
            Assert.That(actual, Is.TypeOf(expected.GetType()));
            Assert.That(expected.Data.Count, Is.EqualTo(actual.Data.Count));
            Assert.IsTrue(actual.Data.SequenceEqual(expected.Data, new TableByTypeDtoComparer()));
        }
        #endregion Unit Tests

        #region Private member methods
        private ITableRepository SetUpTableRepository()
        {
            var mockTableRepository = new Mock<ITableRepository>();
            mockTableRepository
                .Setup(m => m.GetAllAsync(It.IsAny<List<Expression<Func<Table, bool>>>>(),
                    It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
                    It.IsAny<string>()))
                .ReturnsAsync(
                (List<Expression<Func<Table, bool>>> filters,
                    Func<IQueryable<Table>, IOrderedQueryable<Table>> orderBy,
                    string includeString)
                =>
                {
                    var query = _tables.AsQueryable();

                    if (filters is not null)
                    {
                        foreach (var filter in filters)
                        {
                            query = query.Where(filter);
                        }
                    }
                    return orderBy is not null
                            ? new List<Table>(orderBy(query).ToList())
                            : new List<Table>(query.ToList());
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
            mapperMock.Setup(m => m.Map<PaginatedList<Table>, PaginatedList<TableDto>>(It.IsAny<PaginatedList<Table>>()))
                .Returns((PaginatedList<Table> entities) =>
                {
                    var dtos = new List<TableDto>();
                    foreach (var entity in entities)
                    {
                        dtos.Add(new TableDto
                        {
                            Id = entity.Id,
                            NumOfSeats = entity.NumOfSeats,
                            Status = entity.Status,
                            TableType = _mapper.Map<TableTypeDto>(entity.TableType)
                        });
                    }
                    return new PaginatedList<TableDto>(entities.PageNumber, entities.TotalPages, entities.TotalPages, dtos);
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
