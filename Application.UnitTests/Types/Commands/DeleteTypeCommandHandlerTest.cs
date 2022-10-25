using Application.Types.Commands;
using Application.Types.Response;
using Application.Models;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;
using System.Net;

namespace Application.UnitTests.Types.Commands
{
    [TestFixture]
    public class DeleteTypeCommandHandlerTest
    {
        private List<Core.Entities.Type> _types;
        private ITypeRepository _TypeRepository;
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;

        [SetUp]
        public void ReInitializeTest()
        {
            _types = DataSource.Types;
            _TypeRepository = SetUpTypeRepository();
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(x => x.TypeRepository).Returns(_TypeRepository);
            _unitOfWork = unitOfWork.Object;
            _mapper = SetUpMapper();
        }

        [TearDown]
        public void DisposeTest()
        {
            _TypeRepository = null;
            _unitOfWork = null;
            _types = null;
        }

        #region Unit Tests
        [TestCase(2)]
        public async Task Should_Remove_Type(int id)
        {
            //Arrange
            var request = new DeleteTypeCommand()
            {
                Id = id
            };
            var handler = new DeleteTypeCommandHandler(_unitOfWork, _mapper);
            var expected = new Response<TypeDto>()
            {
                StatusCode = HttpStatusCode.NoContent
            };
            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.That(actual.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            var inDatabase = _types.FirstOrDefault(e => e.Id == id);
            Assert.Null(inDatabase);
            Assert.Null(actual.Data);
        }
        #endregion Unit Tests

        #region Private member methods
        private ITypeRepository SetUpTypeRepository()
        {
            var mockTypeRepository = new Mock<ITypeRepository>();
            mockTypeRepository.Setup(m => m.DeleteAsync(It.IsAny<Expression<Func<Core.Entities.Type, bool>>>()))
                .ReturnsAsync(
                (Expression<Func<Core.Entities.Type, bool>> expression)
                =>
                {
                    var inDatabase = _types.AsQueryable().FirstOrDefault(expression);
                    if (inDatabase is not null)
                    {
                        _types.Remove(inDatabase);
                    }
                    return true;
                });
            return mockTypeRepository.Object;
        }

        private IMapper SetUpMapper()
        {
            var mapperMock = new Mock<IMapper>();
            return mapperMock.Object;
        }
        #endregion

    }
}
