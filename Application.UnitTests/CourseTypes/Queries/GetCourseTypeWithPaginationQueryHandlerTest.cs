using Application.CourseTypes.Queries;
using Application.CourseTypes.Response;
using Application.Models;
using AutoMapper;
using Core.Common;
using Core.Entities;
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
        [Test]
        public async Task Should_Return_All()
        {
            //Arrange
            var request = new GetCourseTypeWithPaginationQuery()
            {

            };
            var handler = new GetCourseTypeWithPaginationQueryHandler(_unitOfWork, _mapper);
            var expectedList = new List<CourseTypeDto>();
            foreach (var courseTypeDto in _courseTypes)
            {
                expectedList.Add(new CourseTypeDto
                {
                    Id = courseTypeDto.Id,
                    Name = courseTypeDto.Name
                });
            }

            var expected = new Response<List<CourseTypeDto>>(expectedList);
            //Act
            var result = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(_courseTypes.Count, Is.EqualTo(result.Data.Count));
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
                            query.Where(filter);
                        }
                    }

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
