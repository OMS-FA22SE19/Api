using Application.Common.Exceptions;
using Application.TableTypes.Queries;
using Application.TableTypes.Response;
using Application.Models;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;
using System.Net;

namespace Application.UnitTests.TableTypes.Queries
{
    [TestFixture]
    public class GetTableTypeWithIdQueryHandlerTest
    {
        private List<TableType> _tableTypes;
        private ITableTypeRepository _tableTypeRepository;
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;

        [OneTimeSetUp]
        public void SetUp()
        {
            _tableTypes = DataSource.TableTypes;
        }

        [SetUp]
        public void ReInitializeTest()
        {
            _tableTypeRepository = SetUpTableTypeRepository();
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(x => x.TableTypeRepository).Returns(_tableTypeRepository);
            _unitOfWork = unitOfWork.Object;
            _mapper = SetUpMapper();
        }

        [TearDown]
        public void DisposeTest()
        {
            _tableTypeRepository = null;
            _unitOfWork = null;
        }

        [OneTimeTearDown]
        public void DisposeAllObjects()
        {
            _tableTypes = null;
        }

        #region Unit Tests
        [TestCase(1)]
        public async Task Should_Return_TableType(int id)
        {
            //Arrange
            var request = new GetTableTypeWithIdQuery()
            {
                Id = id
            };
            var handler = new GetTableTypeWithIdQueryHandler(_unitOfWork, _mapper);

            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            var inDatabase = _tableTypes.FirstOrDefault(x => x.Id == id);
            Assert.NotNull(inDatabase);
            var expected = new Response<TableTypeDto>(new TableTypeDto
            {
                Id = inDatabase.Id,
                Name = inDatabase.Name,
                ChargePerSeat = inDatabase.ChargePerSeat,
                CanBeCombined = inDatabase.CanBeCombined
            });
            Assert.That(actual.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(expected.Data, Is.EqualTo(actual.Data));
        }

        [TestCase(10)]
        public async Task Should_Return_Throw_NotFound_Exception(int id)
        {
            //Arrange
            var request = new GetTableTypeWithIdQuery()
            {
                Id = id
            };
            var handler = new GetTableTypeWithIdQueryHandler(_unitOfWork, _mapper);

            //Act
            var ex = Assert.ThrowsAsync<NotFoundException>(async () => await handler.Handle(request, CancellationToken.None));

            //Assert
            Assert.That(ex.Message, Is.EqualTo($"Entity {nameof(TableType)} ({request.Id}) was not found."));
        }
        #endregion Unit Tests

        #region Private member methods
        private ITableTypeRepository SetUpTableTypeRepository()
        {
            var mockTableTypeRepository = new Mock<ITableTypeRepository>();
            mockTableTypeRepository.Setup(m => m.GetAsync(It.IsAny<Expression<Func<TableType, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(
                (Expression<Func<TableType, bool>> filter,
                string includeString)
                =>
                {
                    return _tableTypes.AsQueryable().FirstOrDefault(filter);
                });
            return mockTableTypeRepository.Object;
        }

        private IMapper SetUpMapper()
        {
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<TableTypeDto>(It.IsAny<TableType>()))
                .Returns((TableType tableType) => new TableTypeDto
                {
                    Id = tableType.Id,
                    Name = tableType.Name,
                    ChargePerSeat = tableType.ChargePerSeat,
                    CanBeCombined = tableType.CanBeCombined
                });
            return mapperMock.Object;
        }
        #endregion

    }
}
