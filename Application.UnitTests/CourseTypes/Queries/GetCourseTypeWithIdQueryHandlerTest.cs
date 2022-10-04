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
        [Test]
        public async Task Should_Return_CourseType()
        {
            //Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var mapperMock = new Mock<IMapper>();

            unitOfWorkMock.Setup(m => m.CourseTypeRepository.GetAsync(It.IsAny<Expression<Func<CourseType, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(new CourseType
                {
                    Id = 1,
                    Name = "Starter"
                });
            mapperMock.Setup(m => m.Map<CourseTypeDto>(It.IsAny<CourseType>()))
                .Returns(new CourseTypeDto
                {
                    Id = 1,
                    Name = "Starter"
                });
            var request = new GetCourseTypeWithIdQuery()
            {
                Id = 1
            };

            //Act
            var handler = new GetCourseTypeWithIdQueryHandler(unitOfWorkMock.Object, mapperMock.Object);
            var result = await handler.Handle(request, CancellationToken.None);

            //Assert
            var expected = new Response<CourseTypeDto>(new CourseTypeDto
            {
                Id = 1,
                Name = "Starter"
            });

            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.AreEqual(expected.Data.Id, result.Data.Id);
            Assert.AreEqual(expected.Data.Name, result.Data.Name);
        }

        [Test]
        public async Task Should_Return_Throw_NotFound_Exception()
        {
            //Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var mapperMock = new Mock<IMapper>();
            CourseType expected = null;

            unitOfWorkMock.Setup(m => m.CourseTypeRepository.GetAsync(It.IsAny<Expression<Func<CourseType, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(expected);

            mapperMock.Setup(m => m.Map<CourseTypeDto>(It.IsAny<CourseType>()))
                .Returns(new CourseTypeDto
                {
                    Id = 1,
                    Name = "Starter"
                });
            var request = new GetCourseTypeWithIdQuery()
            {
                Id = 1
            };

            //Act
            var handler = new GetCourseTypeWithIdQueryHandler(unitOfWorkMock.Object, mapperMock.Object);
            var ex = Assert.ThrowsAsync<NotFoundException>(async () => await handler.Handle(request, CancellationToken.None));

            //Assert
            Assert.That(ex.Message, Is.EqualTo($"Entity {nameof(CourseType)} ({request.Id}) was not found."));
        }
    }
}
