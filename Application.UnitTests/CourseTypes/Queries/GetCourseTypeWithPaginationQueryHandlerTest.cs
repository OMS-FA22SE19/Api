using Application.CourseTypes.Queries;
using Application.CourseTypes.Response;
using Application.Common.Models;
using AutoMapper;
using Core.Common;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;
using System.Net;

namespace Application.UnitTests.CourseTypes.Queries
{
    [TestFixture]
    public class GetCourseTypeWithPaginationQueryHandlerTest
    {
        private List<CourseType> _courseTypes;
        private ICourseTypeRepository _courseTypeRepository;
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;

        [OneTimeSetUp]
        public void SetUp()
        {
            _courseTypes = DataSource.CourseTypes;
        }

        [SetUp]
        public void ReInitializeTest()
        {
            _courseTypeRepository = SetUpCourseTypeRepository();
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(x => x.CourseTypeRepository).Returns(_courseTypeRepository);
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
            _courseTypes = null;
        }

        #region Unit Tests

        [TestCase]
        [TestCase(1, 50, "", null, false)]
        [TestCase(1, 2, "", null, false)]
        [TestCase(2, 2, "", null, false)]
        [TestCase(1, 50, "", CourseTypeProperty.Id, false)]
        [TestCase(1, 50, "", CourseTypeProperty.Id, true)]
        [TestCase(1, 50, "", CourseTypeProperty.Name, false)]
        [TestCase(1, 50, "", CourseTypeProperty.Name, true)]
        [TestCase(1, 50, "2", null, false)]
        [TestCase(1, 50, "start", null, false)]
        public async Task Should_Return_With_Condition(
            int pageIndex = 1,
            int pageSize = 50,
            string searchValue = "",
            CourseTypeProperty? orderBy = null,
            bool IsDescending = false)
        {
            //Arrange
            var request = new GetCourseTypeWithPaginationQuery()
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                SearchValue = searchValue,
                OrderBy = orderBy,
                IsDescending = IsDescending
            };
            var handler = new GetCourseTypeWithPaginationQueryHandler(_unitOfWork, _mapper);
            var conditionedList = _courseTypes;
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                conditionedList = conditionedList.Where(e => e.Name.Contains(request.SearchValue) || request.SearchValue.Contains(e.Id.ToString())).ToList();
            }

            conditionedList = conditionedList.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            switch (orderBy)
            {
                case (CourseTypeProperty.Name):
                    if (request.IsDescending)
                    {
                        conditionedList = conditionedList.OrderByDescending(x => x.Name).ToList();
                        break;
                    }
                    conditionedList = conditionedList.OrderBy(x => x.Name).ToList();
                    break;
                case (CourseTypeProperty.Id):
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
            var expectedList = new List<CourseTypeDto>();
            foreach (var courseTypeDto in conditionedList)
            {
                expectedList.Add(new CourseTypeDto
                {
                    Id = courseTypeDto.Id,
                    Name = courseTypeDto.Name
                });
            }

            var expected = new Response<PaginatedList<CourseTypeDto>>(new PaginatedList<CourseTypeDto>(expectedList, _courseTypes.Count, pageIndex, pageSize));
            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.That(actual.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(actual.Data, Is.Not.Null);
            Assert.That(actual, Is.TypeOf(expected.GetType()));
            Assert.That(expected.Data.Count, Is.EqualTo(actual.Data.Count));
            Assert.IsTrue(actual.Data.SequenceEqual(expected.Data, new CourseTypeDtoComparer()));
        }
        #endregion Unit Tests

        #region Private member methods
        private ICourseTypeRepository SetUpCourseTypeRepository()
        {
            var mockCourseTypeRepository = new Mock<ICourseTypeRepository>();
            mockCourseTypeRepository
                .Setup(m => m.GetPaginatedListAsync(It.IsAny<List<Expression<Func<CourseType, bool>>>>()
                    , It.IsAny<Func<IQueryable<CourseType>, IOrderedQueryable<CourseType>>>()
                    , It.IsAny<string>()
                    , It.IsAny<int>()
                    , It.IsAny<int>()))
                .ReturnsAsync(
                (List<Expression<Func<CourseType, bool>>> filters,
                    Func<IQueryable<CourseType>, IOrderedQueryable<CourseType>> orderBy,
                    string includeProperties,
                    int pageIndex,
                    int pageSize)
                =>
                {
                    var query = _courseTypes.AsQueryable();

                    if (filters is not null)
                    {
                        foreach (var filter in filters)
                        {
                            query = query.Where(filter);
                        }
                    }

                    query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);

                    return orderBy is not null
                            ? new PaginatedList<CourseType>(orderBy(query).ToList(), query.Count(), pageIndex, pageSize)
                            : new PaginatedList<CourseType>(query.ToList(), query.Count(), pageIndex, pageSize);
                });
            return mockCourseTypeRepository.Object;
        }

        private IMapper SetUpMapper()
        {
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<PaginatedList<CourseType>, PaginatedList<CourseTypeDto>>(It.IsAny<PaginatedList<CourseType>>()))
                .Returns((PaginatedList<CourseType> entities) =>
                {
                    var dtos = new List<CourseTypeDto>();
                    foreach (var entity in entities)
                    {
                        dtos.Add(new CourseTypeDto
                        {
                            Id = entity.Id,
                            Name = entity.Name,
                        });
                    }
                    return new PaginatedList<CourseTypeDto>(entities.PageNumber, entities.TotalPages, entities.TotalPages, dtos);
                });
            return mapperMock.Object;
        }
        #endregion
    }
}
