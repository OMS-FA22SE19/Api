using Application.CourseTypes.Commands;
using Application.CourseTypes.Response;
using Application.Models;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Moq;
using NUnit.Framework;
using System.Net;

namespace Application.UnitTests.CourseTypes.Commands
{
    [TestFixture]
    public class CreateCourseTypeCommandHandlerTest
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
        [TestCase("abcdef")]
        [TestCase("random name")]
        public async Task Should_Create_CourseType(string name)
        {
            //Arrange
            var request = new CreateCourseTypeCommand()
            {
                Name = name
            };
            var handler = new CreateCourseTypeCommandHandler(_unitOfWork, _mapper);
            var expected = new Response<CourseTypeDto>(new CourseTypeDto { Id = _courseTypes.Max(e => e.Id) + 1, Name = name })
            {
                StatusCode = HttpStatusCode.Created
            };
            var count = _courseTypes.Count + 1;
            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.That(actual.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(_courseTypes.Count, Is.EqualTo(count));
            var inDatabase = _courseTypes.FirstOrDefault(e => e.Name.Equals(name));
            Assert.NotNull(inDatabase);
            Assert.That(actual.Data, Is.EqualTo(expected.Data));
        }
        #endregion Unit Tests

        #region Private member methods
        private ICourseTypeRepository SetUpCourseTypeRepository()
        {
            var mockCourseTypeRepository = new Mock<ICourseTypeRepository>();
            mockCourseTypeRepository.Setup(m => m.InsertAsync(It.IsAny<CourseType>()))
                .ReturnsAsync(
                (CourseType courseType)
                =>
                {
                    courseType.Id = _courseTypes.Max(e => e.Id) + 1;
                    _courseTypes.Add(courseType);
                    return courseType;
                });
            return mockCourseTypeRepository.Object;
        }

        private IMapper SetUpMapper()
        {
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<CourseType>(It.IsAny<CreateCourseTypeCommand>()))
                .Returns((CreateCourseTypeCommand command) => new CourseType
                {
                    Name = command.Name
                });
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
