using Application.Common.Exceptions;
using Application.CourseTypes.Queries;
using Application.CourseTypes.Response;
using Application.Models;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;
using System.Net;

namespace Application.UnitTests.CourseTypes.Queries
{
    [TestFixture]
    public class GetCourseTypeWithIdQueryHandlerTest
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
        [TestCase(1)]
        public async Task Should_Return_CourseType(int id)
        {
            //Arrange
            var request = new GetCourseTypeWithIdQuery()
            {
                Id = id
            };
            var handler = new GetCourseTypeWithIdQueryHandler(_unitOfWork, _mapper);

            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            var inDatabase = _courseTypes.FirstOrDefault(x => x.Id == id);
            Assert.NotNull(inDatabase);
            var expected = new Response<CourseTypeDto>(new CourseTypeDto
            {
                Id = inDatabase.Id,
                Name = inDatabase.Name
            });
            Assert.That(actual.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(expected.Data, Is.EqualTo(actual.Data));
        }

        [TestCase(10)]
        public async Task Should_Return_Throw_NotFound_Exception(int id)
        {
            //Arrange
            var request = new GetCourseTypeWithIdQuery()
            {
                Id = id
            };
            var handler = new GetCourseTypeWithIdQueryHandler(_unitOfWork, _mapper);

            //Act
            var ex = Assert.ThrowsAsync<NotFoundException>(async () => await handler.Handle(request, CancellationToken.None));

            //Assert
            Assert.That(ex.Message, Is.EqualTo($"Entity {nameof(CourseType)} ({request.Id}) was not found."));
        }
        #endregion Unit Tests

        #region Private member methods
        private ICourseTypeRepository SetUpCourseTypeRepository()
        {
            var mockCourseTypeRepository = new Mock<ICourseTypeRepository>();
            mockCourseTypeRepository.Setup(m => m.GetAsync(It.IsAny<Expression<Func<CourseType, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(
                (Expression<Func<CourseType, bool>> filter,
                string includeString)
                =>
                {
                    return _courseTypes.AsQueryable().FirstOrDefault(filter);
                });
            return mockCourseTypeRepository.Object;
        }

        private IMapper SetUpMapper()
        {
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<CourseTypeDto>(It.IsAny<CourseType>()))
                .Returns((CourseType courseType) => new CourseTypeDto
                {
                    Id = courseType.Id,
                    Name = courseType.Name
                });
            return mapperMock.Object;
        }
        #endregion

    }
}
