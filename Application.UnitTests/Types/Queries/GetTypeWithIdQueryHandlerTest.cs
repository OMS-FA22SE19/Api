using Application.Common.Exceptions;
using Application.Models;
using Application.Types.Queries;
using Application.Types.Response;
using AutoMapper;
using Core.Interfaces;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;
using System.Net;

namespace Application.UnitTests.Types.Queries
{
    [TestFixture]
    public class GetTypeWithIdQueryHandlerTest
    {
        private List<Core.Entities.Type> _types;
        private ITypeRepository _TypeRepository;
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;

        [OneTimeSetUp]
        public void SetUp()
        {
            _types = DataSource.Types;
        }

        [SetUp]
        public void ReInitializeTest()
        {
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
        }

        [OneTimeTearDown]
        public void DisposeAllObjects()
        {
            _types = null;
        }

        #region Unit Tests
        [TestCase(1)]
        public async Task Should_Return_Type(int id)
        {
            //Arrange
            var request = new GetTypeWithIdQuery()
            {
                Id = id
            };
            var handler = new GetTypeWithIdQueryHandler(_unitOfWork, _mapper);

            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            var inDatabase = _types.FirstOrDefault(x => x.Id == id);
            Assert.NotNull(inDatabase);
            var expected = new Response<TypeDto>(new TypeDto
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
            var request = new GetTypeWithIdQuery()
            {
                Id = id
            };
            var handler = new GetTypeWithIdQueryHandler(_unitOfWork, _mapper);

            //Act
            var ex = Assert.ThrowsAsync<NotFoundException>(async () => await handler.Handle(request, CancellationToken.None));

            string test = $"Entity {nameof(Core.Entities.Type)} ({request.Id}) was not found.";
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
                (Expression<Func<Core.Entities.Type, bool>> filter,
                string includeString)
                =>
                {
                    return _types.AsQueryable().FirstOrDefault(filter);
                });
            return mockTypeRepository.Object;
        }

        private IMapper SetUpMapper()
        {
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<TypeDto>(It.IsAny<Core.Entities.Type>()))
                .Returns((Core.Entities.Type type) => new TypeDto
                {
                    Id = type.Id,
                    Name = type.Name
                });
            return mapperMock.Object;
        }
        #endregion

    }
}
