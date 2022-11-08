using Application.Common.Exceptions;
using Application.Models;
using Application.TableTypes.Commands;
using Application.TableTypes.Response;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;
using System.Net;

namespace Application.UnitTests.TableTypes.Commands
{
    public class UpdateTableTypeCommandHandlerTest
    {
        private List<TableType> _tableTypes;
        private ITableTypeRepository _tableTypeRepository;
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;

        [SetUp]
        public void ReInitializeTest()
        {
            _tableTypes = DataSource.TableTypes;
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
            _tableTypes = null;
        }

        #region Unit Tests
        [TestCase(2, "abcdef", 12345, false)]
        public async Task Should_Update_TableType(int id, string name, double chargePerSeat, bool canBeCombined)
        {
            //Arrange
            var request = new UpdateTableTypeCommand()
            {
                Id = id,
                Name = name,
                ChargePerSeat = chargePerSeat,
                CanBeCombined = canBeCombined
            };
            var handler = new UpdateTableTypeCommandHandler(_unitOfWork, _mapper);
            var expected = new TableType()
            {
                Id = id,
                Name = name,
                ChargePerSeat = chargePerSeat,
                CanBeCombined = canBeCombined
            };
            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.That(actual.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(actual, Is.TypeOf(typeof(Response<TableTypeDto>)));
            Assert.Null(actual.Data);
            var inDatabase = _tableTypes.FirstOrDefault(e => e.Id == id);
            Assert.That(inDatabase, Is.EqualTo(expected));
        }
        [TestCase(10)]
        public async Task Should_Return_Throw_NotFound_Exception(int id)
        {
            //Arrange
            var request = new UpdateTableTypeCommand()
            {
                Id = id
            };
            var handler = new UpdateTableTypeCommandHandler(_unitOfWork, _mapper);

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
                (Expression<Func<TableType, bool>> expression,
                string includeProperties)
                =>
                {
                    return _tableTypes.AsQueryable().FirstOrDefault(expression);
                });
            mockTableTypeRepository.Setup(m => m.UpdateAsync(It.IsAny<TableType>()))
                .ReturnsAsync(
                (TableType updatedEntity)
                =>
                {
                    var inDatabase = _tableTypes.FirstOrDefault(e => e.Id == updatedEntity.Id);
                    inDatabase.Name = updatedEntity.Name;
                    inDatabase.ChargePerSeat = updatedEntity.ChargePerSeat;
                    inDatabase.CanBeCombined = updatedEntity.CanBeCombined;
                    return inDatabase;
                });
            return mockTableTypeRepository.Object;
        }

        private IMapper SetUpMapper()
        {
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<TableType>(It.IsAny<UpdateTableTypeCommand>()))
                .Returns((UpdateTableTypeCommand command) => new TableType
                {
                    Id = command.Id,
                    Name = command.Name
                });
            return mapperMock.Object;
        }
        #endregion
    }
}
