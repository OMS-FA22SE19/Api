using Application.Types.Commands;
using Application.Types.Response;
using Application.Models;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Moq;
using NUnit.Framework;
using System.Net;

namespace Application.UnitTests.Types.Commands
{
    [TestFixture]
    public class CreateTypeCommandHandlerTest
    {
        private List<Core.Entities.Type> _Types;
        private ITypeRepository _TypeRepository;
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;

        [SetUp]
        public void ReInitializeTest()
        {
            _Types = DataSource.Types;
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
            _Types = null;
        }

        #region Unit Tests
        [TestCase("abcdef")]
        public async Task Should_Create_Type(string name)
        {
            //Arrange
            var request = new CreateTypeCommand()
            {
                Name = name
            };
            var handler = new CreateTypeCommandHandler(_unitOfWork, _mapper);
            var expected = new Response<TypeDto>(new TypeDto { Id = _Types.Max(e => e.Id) + 1, Name = name })
            {
                StatusCode = HttpStatusCode.Created
            };
            var count = _Types.Count + 1;
            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.That(actual.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(_Types.Count, Is.EqualTo(count));
            var inDatabase = _Types.FirstOrDefault(e => e.Name.Equals(name));
            Assert.NotNull(inDatabase);
            Assert.That(actual.Data, Is.EqualTo(expected.Data));
        }
        #endregion Unit Tests

        #region Private member methods
        private ITypeRepository SetUpTypeRepository()
        {
            var mockTypeRepository = new Mock<ITypeRepository>();
            mockTypeRepository.Setup(m => m.InsertAsync(It.IsAny<Core.Entities.Type>()))
                .ReturnsAsync(
                (Core.Entities.Type Type)
                =>
                {
                    Type.Id = _Types.Max(e => e.Id) + 1;
                    _Types.Add(Type);
                    return Type;
                });
            return mockTypeRepository.Object;
        }

        private IMapper SetUpMapper()
        {
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<Core.Entities.Type>(It.IsAny<CreateTypeCommand>()))
                .Returns((CreateTypeCommand command) => new Core.Entities.Type
                {
                    Name = command.Name
                });
            mapperMock.Setup(m => m.Map<TypeDto>(It.IsAny<Core.Entities.Type>()))
                .Returns((Core.Entities.Type Type) => new TypeDto
                {
                    Id = Type.Id,
                    Name = Type.Name
                });
            return mapperMock.Object;
        }
        #endregion

    }
}
