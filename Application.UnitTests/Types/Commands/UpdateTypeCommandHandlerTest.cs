using Application.Common.Exceptions;
using Application.Models;
using Application.Types.Commands;
using Application.Types.Response;
using AutoMapper;
using Core.Interfaces;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;
using System.Net;

namespace Application.UnitTests.Types.Commands
{
    public class UpdateTypeCommandHandlerTest
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
        [TestCase(2, "abcdef")]
        [TestCase(1, "abcdef")]
        [TestCase(3, "random name")]
        [TestCase(1, "random name")]
        [TestCase(2, "random name")]
        public async Task Should_Update_Type(int id, string name)
        {
            //Arrange
            var request = new UpdateTypeCommand()
            {
                Id = id,
                Name = name
            };
            var handler = new UpdateTypeCommandHandler(_unitOfWork, _mapper);
            var expected = new Core.Entities.Type()
            {
                Id = id,
                Name = name,
                IsDeleted = false
            };
            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.That(actual.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(actual, Is.TypeOf(typeof(Response<TypeDto>)));
            Assert.Null(actual.Data);
            var inDatabase = _types.FirstOrDefault(e => e.Id == id);
            Assert.That(inDatabase, Is.EqualTo(expected));
        }
        [TestCase(10)]
        [TestCase(0)]
        public async Task Should_Return_Throw_NotFound_Exception(int id)
        {
            //Arrange
            var request = new UpdateTypeCommand()
            {
                Id = id
            };
            var handler = new UpdateTypeCommandHandler(_unitOfWork, _mapper);

            //Act
            var ex = Assert.ThrowsAsync<NotFoundException>(async () => await handler.Handle(request, CancellationToken.None));

            //Assert
            Assert.That(ex.Message, Is.EqualTo($"Entity {nameof(Core.Entities.Type)} ({request.Id}) was not found."));
        }
        #endregion Unit Tests

        #region Private member methods
        private ITypeRepository SetUpTypeRepository()
        {
            var mockTypeRepository = new Mock<ITypeRepository>();
            mockTypeRepository.Setup(m => m.GetAsync(It.IsAny<Expression<Func<Core.Entities.Type, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(
                (Expression<Func<Core.Entities.Type, bool>> expression,
                string includeProperties)
                =>
                {
                    return _types.AsQueryable().FirstOrDefault(expression);
                });
            mockTypeRepository.Setup(m => m.UpdateAsync(It.IsAny<Core.Entities.Type>()))
                .ReturnsAsync(
                (Core.Entities.Type updatedEntity)
                =>
                {
                    var inDatabase = _types.FirstOrDefault(e => e.Id == updatedEntity.Id);
                    inDatabase.Name = updatedEntity.Name;
                    return inDatabase;
                });
            return mockTypeRepository.Object;
        }

        private IMapper SetUpMapper()
        {
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<Core.Entities.Type>(It.IsAny<UpdateTypeCommand>()))
                .Returns((UpdateTypeCommand command) => new Core.Entities.Type
                {
                    Id = command.Id,
                    Name = command.Name
                });
            return mapperMock.Object;
        }
        #endregion
    }
}
