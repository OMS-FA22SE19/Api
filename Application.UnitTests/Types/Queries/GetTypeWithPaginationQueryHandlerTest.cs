using Application.Common.Models;
using Application.Types.Queries;
using Application.Types.Response;
using AutoMapper;
using Core.Common;
using Core.Enums;
using Core.Interfaces;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;
using System.Net;

namespace Application.UnitTests.Types.Queries
{
    [TestFixture]
    public class GetTypeWithPaginationQueryHandlerTest
    {
        private List<Core.Entities.Type> _types;
        private ITypeRepository _courseTypeRepository;
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;

        [OneTimeSetUp]
        public void SetUp()
        {
            _types = DataSource.Types;
        }

        [SetUp]
        public void ReInitializeTest()
        {
            _courseTypeRepository = SetUpTypeRepository();
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(x => x.TypeRepository).Returns(_courseTypeRepository);
            _unitOfWork = unitOfWork.Object;
            _mapper = SetUpMapper();
        }

        [TearDown]
        public void DisposeTest()
        {
            _courseTypeRepository = null;
            _unitOfWork = null;
        }

        [OneTimeTearDown]
        public void DisposeAllObjects()
        {
            _types = null;
        }

        #region Unit Tests

        [TestCase]
        [TestCase(1, 50, "", null, false)]
        [TestCase(1, 2, "", null, false)]
        [TestCase(2, 2, "", null, false)]
        [TestCase(1, 50, "", TypeProperty.Id, false)]
        [TestCase(1, 50, "", TypeProperty.Id, true)]
        [TestCase(1, 50, "", TypeProperty.Name, false)]
        [TestCase(1, 50, "", TypeProperty.Name, true)]
        [TestCase(1, 50, "6", null, false)]
        [TestCase(1, 50, "Diary", null, false)]
        public async Task Should_Return_With_Condition(
            int pageIndex = 1,
            int pageSize = 50,
            string searchValue = "",
            TypeProperty? orderBy = null,
            bool IsDescending = false)
        {
            //Arrange
            var request = new GetTypeWithPaginationQuery()
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                SearchValue = searchValue,
                OrderBy = orderBy,
                IsDescending = IsDescending
            };
            var handler = new GetTypeWithPaginationQueryHandler(_unitOfWork, _mapper);
            var conditionedList = _types;
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                conditionedList = conditionedList.Where(e => e.Name.Contains(request.SearchValue) || request.SearchValue.Contains(e.Id.ToString())).ToList();
            }

            conditionedList = conditionedList.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            switch (orderBy)
            {
                case (TypeProperty.Name):
                    if (request.IsDescending)
                    {
                        conditionedList = conditionedList.OrderByDescending(x => x.Name).ToList();
                        break;
                    }
                    conditionedList = conditionedList.OrderBy(x => x.Name).ToList();
                    break;
                case (TypeProperty.Id):
                    if (request.IsDescending)
                    {
                        conditionedList = conditionedList.OrderByDescending(x => x.Id).ToList();
                        break;
                    }
                    conditionedList = conditionedList.OrderBy(x => x.Id).ToList();
                    break;
                default:
                    break;
            }
            var expectedList = new List<TypeDto>();
            foreach (var TypeDto in conditionedList)
            {
                expectedList.Add(new TypeDto
                {
                    Id = TypeDto.Id,
                    Name = TypeDto.Name
                });
            }

            var expected = new Response<PaginatedList<TypeDto>>(new PaginatedList<TypeDto>(expectedList, _types.Count, pageIndex, pageSize));
            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.That(actual.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(actual.Data, Is.Not.Null);
            Assert.That(actual, Is.TypeOf(expected.GetType()));
            Assert.That(expected.Data.Count, Is.EqualTo(actual.Data.Count));
            Assert.IsTrue(actual.Data.SequenceEqual(expected.Data, new TypeDtoComparer()));
        }
        #endregion Unit Tests

        #region Private member methods
        private ITypeRepository SetUpTypeRepository()
        {
            var mockTypeRepository = new Mock<ITypeRepository>();
            mockTypeRepository
                .Setup(m => m.GetPaginatedListAsync(It.IsAny<List<Expression<Func<Core.Entities.Type, bool>>>>()
                    , It.IsAny<Func<IQueryable<Core.Entities.Type>, IOrderedQueryable<Core.Entities.Type>>>()
                    , It.IsAny<string>()
                    , It.IsAny<int>()
                    , It.IsAny<int>()))
                .ReturnsAsync(
                (List<Expression<Func<Core.Entities.Type, bool>>> filters,
                    Func<IQueryable<Core.Entities.Type>, IOrderedQueryable<Core.Entities.Type>> orderBy,
                    string includeProperties,
                    int pageIndex,
                    int pageSize)
                =>
                {
                    var query = _types.AsQueryable();

                    if (filters is not null)
                    {
                        foreach (var filter in filters)
                        {
                            query = query.Where(filter);
                        }
                    }

                    query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);

                    return orderBy is not null
                            ? new PaginatedList<Core.Entities.Type>(orderBy(query).ToList(), query.Count(), pageIndex, pageSize)
                            : new PaginatedList<Core.Entities.Type>(query.ToList(), query.Count(), pageIndex, pageSize);
                });
            return mockTypeRepository.Object;
        }

        private IMapper SetUpMapper()
        {
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<PaginatedList<Core.Entities.Type>, PaginatedList<TypeDto>>(It.IsAny<PaginatedList<Core.Entities.Type>>()))
                .Returns((PaginatedList<Core.Entities.Type> entities) =>
                {
                    var dtos = new List<TypeDto>();
                    foreach (var entity in entities)
                    {
                        dtos.Add(new TypeDto
                        {
                            Id = entity.Id,
                            Name = entity.Name,
                        });
                    }
                    return new PaginatedList<TypeDto>(entities.PageNumber, entities.TotalPages, entities.TotalPages, dtos);
                });
            return mapperMock.Object;
        }
        #endregion
    }
}
