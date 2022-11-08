using Application.Models;
using Application.Tables.Queries;
using Application.Tables.Response;
using Application.TableTypes.Response;
using AutoMapper;
using Core.Common;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;
using System.Net;

namespace Application.UnitTests.Tables.Queries
{
    [TestFixture]
    public class GetTableWithPaginationQueryHandlerTest
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

        [TestCase]
        [TestCase(1, 50, "", null, false)]
        [TestCase(1, 2, "", null, false)]
        [TestCase(2, 2, "", null, false)]
        [TestCase(1, 50, "", TableProperty.NumOfSeats, false)]
        [TestCase(1, 50, "", TableProperty.NumOfSeats, true)]
        [TestCase(1, 50, "", TableProperty.Status, false)]
        [TestCase(1, 50, "", TableProperty.Status, true)]
        [TestCase(1, 50, "", TableProperty.Type, false)]
        [TestCase(1, 50, "", TableProperty.Type, true)]
        [TestCase(1, 50, "random", null, false)]
        [TestCase(1, 50, "4", null, false)]
        public async Task Should_Return_With_Condition(
            int pageIndex = 1,
            int pageSize = 50,
            string searchValue = "",
            TableProperty? orderBy = null,
            bool IsDescending = false)
        {
            //Arrange
            var request = new GetTableWithPaginationQuery()
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                SearchValue = searchValue,
                OrderBy = orderBy,
                IsDescending = IsDescending
            };
            var handler = new GetTableWithPaginationQueryHandler(_unitOfWork, _mapper);
            var conditionedList = _tables;
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                conditionedList = conditionedList.Where(e => e.TableTypeId.ToString().Contains(request.SearchValue)
                || request.SearchValue.Contains(e.Id.ToString())
                || e.NumOfSeats.ToString().Contains(request.SearchValue)).ToList();
            }

            conditionedList = conditionedList.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            switch (orderBy)
            {
                case (TableProperty.NumOfSeats):
                    if (request.IsDescending)
                    {
                        conditionedList = conditionedList.OrderByDescending(x => x.NumOfSeats).ToList();
                        break;
                    }
                    conditionedList = conditionedList.OrderBy(x => x.NumOfSeats).ToList();
                    break;
                case (TableProperty.Status):
                    if (request.IsDescending)
                    {
                        conditionedList = conditionedList.OrderByDescending(x => x.Status).ToList();
                        break;
                    }
                    conditionedList = conditionedList.OrderBy(x => x.Status).ToList();
                    break;
                case (TableProperty.Type):
                    if (request.IsDescending)
                    {
                        conditionedList = conditionedList.OrderByDescending(x => x.TableType.Name).ToList();
                        break;
                    }
                    conditionedList = conditionedList.OrderBy(x => x.TableType.Name).ToList();
                    break;
                default:
                    break;
            }
            var expectedList = new List<TableDto>();
            foreach (var tableTypeDto in conditionedList)
            {
                var tableType = _tableTypes.Find(tt => tt.Id == tableTypeDto.TableTypeId);
                var mappedTableType = _mapper.Map<TableTypeDto>(tableType);
                expectedList.Add(new TableDto
                {
                    Id = tableTypeDto.Id,
                    NumOfSeats = tableTypeDto.NumOfSeats,
                    Status = tableTypeDto.Status,
                    TableType = mappedTableType
                });
            }

            var expected = new Response<PaginatedList<TableDto>>(new PaginatedList<TableDto>(expectedList, _tables.Count, pageIndex, pageSize));
            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.That(actual.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(actual.Data, Is.Not.Null);
            Assert.That(actual, Is.TypeOf(expected.GetType()));
            Assert.That(expected.Data.Count, Is.EqualTo(actual.Data.Count));
            Assert.IsTrue(actual.Data.SequenceEqual(expected.Data, new TableDtoComparer()));
        }
        #endregion Unit Tests

        #region Private member methods
        private ITableRepository SetUpTableRepository()
        {
            var mockTableRepository = new Mock<ITableRepository>();
            mockTableRepository
                .Setup(m => m.GetPaginatedListAsync(It.IsAny<List<Expression<Func<Table, bool>>>>()
                    , It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>()
                    , It.IsAny<string>()
                    , It.IsAny<int>()
                    , It.IsAny<int>()))
                .ReturnsAsync(
                (List<Expression<Func<Table, bool>>> filters,
                    Func<IQueryable<Table>, IOrderedQueryable<Table>> orderBy,
                    string includeProperties,
                    int pageIndex,
                    int pageSize)
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

                    query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);

                    return orderBy is not null
                            ? new PaginatedList<Table>(orderBy(query).ToList(), query.Count(), pageIndex, pageSize)
                            : new PaginatedList<Table>(query.ToList(), query.Count(), pageIndex, pageSize);
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
