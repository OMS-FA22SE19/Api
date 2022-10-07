using Application.CourseTypes.Commands;
using Application.CourseTypes.Response;
using Application.Models;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;
using System.Net;

namespace Application.UnitTests.CourseTypes.Commands
{
    [TestFixture]
    public class DeleteCourseTypeCommandHandlerTest
    {
        private List<CourseType> _courseTypes;
        private ICourseTypeRepository _courseTypeRepository;
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;

        [SetUp]
        public void ReInitializeTest()
        {
            _courseTypes = DataSource.CourseTypes;
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
            _courseTypes = null;
        }

        #region Unit Tests
        [TestCase(2)]
        public async Task Should_Remove_CourseType(int id)
        {
            //Arrange
            var request = new DeleteCourseTypeCommand()
            {
                Id = id
            };
            var handler = new DeleteCourseTypeCommandHandler(_unitOfWork, _mapper);
            var expected = new Response<CourseTypeDto>()
            {
                StatusCode = HttpStatusCode.NoContent
            };
            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.That(actual.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            var inDatabase = _courseTypes.FirstOrDefault(e => e.Id == id);
            Assert.Null(inDatabase);
            Assert.Null(actual.Data);
        }
        #endregion Unit Tests

        #region Private member methods
        private ICourseTypeRepository SetUpCourseTypeRepository()
        {
            var mockCourseTypeRepository = new Mock<ICourseTypeRepository>();
            mockCourseTypeRepository.Setup(m => m.DeleteAsync(It.IsAny<Expression<Func<CourseType, bool>>>()))
                .ReturnsAsync(
                (Expression<Func<CourseType, bool>> expression)
                =>
                {
                    var inDatabase = _courseTypes.AsQueryable().FirstOrDefault(expression);
                    if (inDatabase is not null)
                    {
                        _courseTypes.Remove(inDatabase);
                    }
                    return true;
                });
            return mockCourseTypeRepository.Object;
        }

        private IMapper SetUpMapper()
        {
            var mapperMock = new Mock<IMapper>();
            return mapperMock.Object;
        }
        #endregion

    }
}
