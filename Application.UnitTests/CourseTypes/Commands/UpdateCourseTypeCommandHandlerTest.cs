using Application.Common.Exceptions;
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
    public class UpdateCourseTypeCommandHandlerTest
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
        [TestCase(2, "abcdef")]
        [TestCase(1, "abcdef")]
        [TestCase(3, "random name")]
        [TestCase(1, "random name")]
        [TestCase(2, "random name")]
        public async Task Should_Update_CourseType(int id, string name)
        {
            //Arrange
            var request = new UpdateCourseTypeCommand()
            {
                Id = id,
                Name = name
            };
            var handler = new UpdateCourseTypeCommandHandler(_unitOfWork, _mapper);
            var expected = new CourseType()
            {
                Id = id,
                Name = name
            };
            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.That(actual.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(actual, Is.TypeOf(typeof(Response<CourseTypeDto>)));
            Assert.Null(actual.Data);
            var inDatabase = _courseTypes.FirstOrDefault(e => e.Id == id);
            Assert.That(inDatabase, Is.EqualTo(expected));
        }
        [TestCase(10)]
        [TestCase(0)]
        public async Task Should_Return_Throw_NotFound_Exception(int id)
        {
            //Arrange
            var request = new UpdateCourseTypeCommand()
            {
                Id = id
            };
            var handler = new UpdateCourseTypeCommandHandler(_unitOfWork, _mapper);

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
                (Expression<Func<CourseType, bool>> expression,
                string includeProperties)
                =>
                {
                    return _courseTypes.AsQueryable().FirstOrDefault(expression);
                });
            mockCourseTypeRepository.Setup(m => m.UpdateAsync(It.IsAny<CourseType>()))
                .ReturnsAsync(
                (CourseType updatedEntity)
                =>
                {
                    var inDatabase = _courseTypes.FirstOrDefault(e => e.Id == updatedEntity.Id);
                    inDatabase.Name = updatedEntity.Name;
                    return inDatabase;
                });
            return mockCourseTypeRepository.Object;
        }

        private IMapper SetUpMapper()
        {
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<CourseType>(It.IsAny<UpdateCourseTypeCommand>()))
                .Returns((UpdateCourseTypeCommand command) => new CourseType
                {
                    Id = command.Id,
                    Name = command.Name
                });
            return mapperMock.Object;
        }
        #endregion
    }
}
