using Application.Models;
using Application.TableTypes.Commands;
using Application.TableTypes.Response;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Moq;
using NUnit.Framework;
using System.Net;

namespace Application.UnitTests.TableTypes.Commands
{
    [TestFixture]
    public class CreateTableTypeCommandHandlerTest
    {
        private List<TableType> _tableTypes;
        private ITableTypeRepository _TableTypeRepository;
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;

        [SetUp]
        public void ReInitializeTest()
        {
            _tableTypes = DataSource.TableTypes;
            _TableTypeRepository = SetUpTableTypeRepository();
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(x => x.TableTypeRepository).Returns(_TableTypeRepository);
            _unitOfWork = unitOfWork.Object;
            _mapper = SetUpMapper();
        }

        [TearDown]
        public void DisposeTest()
        {
            _TableTypeRepository = null;
            _unitOfWork = null;
            _tableTypes = null;
        }

        #region Unit Tests
        [TestCase("abcdef", 12345, true)]
        public async Task Should_Create_TableType(string name, int chargePerSeat, bool canBeCombined)
        {
            //Arrange
            var request = new CreateTableTypeCommand()
            {
                Name = name,
                ChargePerSeat = chargePerSeat,
                CanBeCombined = canBeCombined
            };
            var handler = new CreateTableTypeCommandHandler(_unitOfWork, _mapper);
            var expected = new Response<TableTypeDto>(new TableTypeDto { Id = _tableTypes.Max(e => e.Id) + 1, Name = name, ChargePerSeat = chargePerSeat, CanBeCombined = canBeCombined })
            {
                StatusCode = HttpStatusCode.Created
            };
            var count = _tableTypes.Count + 1;
            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.That(actual.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(_tableTypes.Count, Is.EqualTo(count));
            var inDatabase = _tableTypes.FirstOrDefault(e => e.Name.Equals(name));
            Assert.NotNull(inDatabase);
            Assert.That(actual.Data, Is.EqualTo(expected.Data));
        }
        #endregion Unit Tests

        #region Private member methods
        private ITableTypeRepository SetUpTableTypeRepository()
        {
            var mockTableTypeRepository = new Mock<ITableTypeRepository>();
            mockTableTypeRepository.Setup(m => m.InsertAsync(It.IsAny<TableType>()))
                .ReturnsAsync(
                (TableType TableType)
                =>
                {
                    TableType.Id = _tableTypes.Max(e => e.Id) + 1;
                    _tableTypes.Add(TableType);
                    return TableType;
                });
            return mockTableTypeRepository.Object;
        }

        private IMapper SetUpMapper()
        {
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<TableType>(It.IsAny<CreateTableTypeCommand>()))
                .Returns((CreateTableTypeCommand command) => new TableType
                {
                    Name = command.Name,
                    ChargePerSeat = command.ChargePerSeat,
                    CanBeCombined = command.CanBeCombined
                });
            mapperMock.Setup(m => m.Map<TableTypeDto>(It.IsAny<TableType>()))
                .Returns((TableType TableType) => new TableTypeDto
                {
                    Id = TableType.Id,
                    Name = TableType.Name,
                    ChargePerSeat = TableType.ChargePerSeat,
                    CanBeCombined = TableType.CanBeCombined
                });
            return mapperMock.Object;
        }
        #endregion

    }
}
