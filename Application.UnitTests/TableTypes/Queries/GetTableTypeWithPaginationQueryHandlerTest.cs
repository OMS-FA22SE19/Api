using Application.Common.Models;
using Application.TableTypes.Queries;
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

namespace Application.UnitTests.TableTypes.Queries
{
    [TestFixture]
    public class GetTableTypeWithPaginationQueryHandlerTest
    {
        private List<TableType> _tableTypes;
        private List<Table> _tables;
        private ITableTypeRepository _tableTypeRepository;
        private ITableRepository _tableRepository;
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;

        [OneTimeSetUp]
        public void SetUp()
        {
            _tableTypes = DataSource.TableTypes;
            _tables = DataSource.Tables;
        }

        [SetUp]
        public void ReInitializeTest()
        {
            _tableTypeRepository = SetUpTableTypeRepository();
            _tableRepository = SetUpTableRepository();
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(x => x.TableTypeRepository).Returns(_tableTypeRepository);
            unitOfWork.SetupGet(x => x.TableRepository).Returns(_tableRepository);
            _unitOfWork = unitOfWork.Object;
            _mapper = SetUpMapper();
        }

        [TearDown]
        public void DisposeTest()
        {
            _tableTypeRepository = null;
            _unitOfWork = null;
        }

        [OneTimeTearDown]
        public void DisposeAllObjects()
        {
            _tableTypes = null;
        }

        #region Unit Tests

        [TestCase]
        [TestCase(1, 50, "", null, false)]
        [TestCase(1, 2, "", null, false)]
        [TestCase(2, 2, "", null, false)]
        [TestCase(1, 50, "", TableTypeProperty.Id, false)]
        [TestCase(1, 50, "", TableTypeProperty.Id, true)]
        [TestCase(1, 50, "", TableTypeProperty.Name, false)]
        [TestCase(1, 50, "", TableTypeProperty.Name, true)]
        [TestCase(1, 50, "", TableTypeProperty.ChargePerSeat, false)]
        [TestCase(1, 50, "", TableTypeProperty.ChargePerSeat, true)]
        [TestCase(1, 50, "random", null, false)]
        [TestCase(1, 50, "door", null, false)]
        public async Task Should_Return_With_Condition(
            int pageIndex = 1,
            int pageSize = 50,
            string searchValue = "",
            TableTypeProperty? orderBy = null,
            bool IsDescending = false)
        {
            //Arrange
            var request = new GetTableTypeWithPaginationQuery()
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                SearchValue = searchValue,
                OrderBy = orderBy,
                IsDescending = IsDescending
            };
            var handler = new GetTableTypeWithPaginationQueryHandler(_unitOfWork, _mapper);
            var conditionedList = _tableTypes;
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                conditionedList = conditionedList.Where(e => e.Name.Contains(request.SearchValue) || request.SearchValue.Contains(e.Id.ToString())).ToList();
            }

            conditionedList = conditionedList.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            switch (orderBy)
            {
                case (TableTypeProperty.Name):
                    if (request.IsDescending)
                    {
                        conditionedList = conditionedList.OrderByDescending(x => x.Name).ToList();
                        break;
                    }
                    conditionedList = conditionedList.OrderBy(x => x.Name).ToList();
                    break;
                case (TableTypeProperty.Id):
                    if (request.IsDescending)
                    {
                        conditionedList = conditionedList.OrderByDescending(x => x.Id).ToList();
                        break;
                    }
                    conditionedList = conditionedList.OrderBy(x => x.Id).ToList();
                    break;
                case (TableTypeProperty.ChargePerSeat):
                    if (request.IsDescending)
                    {
                        conditionedList = conditionedList.OrderByDescending(x => x.ChargePerSeat).ToList();
                        break;
                    }
                    conditionedList = conditionedList.OrderBy(x => x.ChargePerSeat).ToList();
                    break;
                default:
                    break;
            }
            var expectedList = new List<TableTypeDto>();
            foreach (var tableTypeDto in conditionedList)
            {
                expectedList.Add(new TableTypeDto
                {
                    Id = tableTypeDto.Id,
                    Name = tableTypeDto.Name,
                    ChargePerSeat = tableTypeDto.ChargePerSeat,
                    CanBeCombined = tableTypeDto.CanBeCombined
                });
            }

            var expected = new Response<PaginatedList<TableTypeDto>>(new PaginatedList<TableTypeDto>(expectedList, _tableTypes.Count, pageIndex, pageSize));
            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.That(actual.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(actual.Data, Is.Not.Null);
            Assert.That(actual, Is.TypeOf(expected.GetType()));
            Assert.That(expected.Data.Count, Is.EqualTo(actual.Data.Count));
            Assert.IsTrue(actual.Data.SequenceEqual(expected.Data, new TableTypeDtoComparer()));
        }
        #endregion Unit Tests

        #region Private member methods
        private ITableTypeRepository SetUpTableTypeRepository()
        {
            var mockTableTypeRepository = new Mock<ITableTypeRepository>();
            mockTableTypeRepository
                .Setup(m => m.GetPaginatedListAsync(It.IsAny<List<Expression<Func<TableType, bool>>>>()
                    , It.IsAny<Func<IQueryable<TableType>, IOrderedQueryable<TableType>>>()
                    , It.IsAny<string>()
                    , It.IsAny<int>()
                    , It.IsAny<int>()))
                .ReturnsAsync(
                (List<Expression<Func<TableType, bool>>> filters,
                    Func<IQueryable<TableType>, IOrderedQueryable<TableType>> orderBy,
                    string includeProperties,
                    int pageIndex,
                    int pageSize)
                =>
                {
                    var query = _tableTypes.AsQueryable();

                    if (filters is not null)
                    {
                        foreach (var filter in filters)
                        {
                            query = query.Where(filter);
                        }
                    }

                    query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);

                    return orderBy is not null
                            ? new PaginatedList<TableType>(orderBy(query).ToList(), query.Count(), pageIndex, pageSize)
                            : new PaginatedList<TableType>(query.ToList(), query.Count(), pageIndex, pageSize);
                });
            return mockTableTypeRepository.Object;
        }

        private ITableRepository SetUpTableRepository()
        {
            var mockTableRepository = new Mock<ITableRepository>();
            mockTableRepository.Setup(m => m.GetAllAsync(
                It.IsAny<List<Expression<Func<Table, bool>>>>(),
                It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
                It.IsAny<string>()))
                .ReturnsAsync(
                (List<Expression<Func<Table, bool>>> filters,
                Func<IQueryable<Table>, IOrderedQueryable<Table>> orderBy,
                string includeString)
                =>
                {
                    var query = _tables.AsQueryable();
                    foreach (var filter in filters)
                    {
                        query = query.Where(filter);
                    }
                    return query.ToList();
                });
            return mockTableRepository.Object;
        }

        private IMapper SetUpMapper()
        {
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<PaginatedList<TableType>, PaginatedList<TableTypeDto>>(It.IsAny<PaginatedList<TableType>>()))
                .Returns((PaginatedList<TableType> entities) =>
                {
                    var dtos = new List<TableTypeDto>();
                    foreach (var entity in entities)
                    {
                        dtos.Add(new TableTypeDto
                        {
                            Id = entity.Id,
                            Name = entity.Name,
                            ChargePerSeat = entity.ChargePerSeat,
                            CanBeCombined = entity.CanBeCombined
                        });
                    }
                    return new PaginatedList<TableTypeDto>(entities.PageNumber, entities.TotalPages, entities.TotalPages, dtos);
                });
            return mapperMock.Object;
        }
        #endregion
    }
}
