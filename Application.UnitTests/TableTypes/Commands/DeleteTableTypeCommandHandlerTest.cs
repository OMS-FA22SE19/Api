using Application.Common.Models;
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
    [TestFixture]
    public class DeleteTableTypeCommandHandlerTest
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
        [TestCase(2)]
        public async Task Should_Remove_TableType(int id)
        {
            //Arrange
            var request = new DeleteTableTypeCommand()
            {
                Id = id
            };
            var handler = new DeleteTableTypeCommandHandler(_unitOfWork, _mapper);
            var expected = new Response<TableTypeDto>()
            {
                StatusCode = HttpStatusCode.NoContent
            };
            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.That(actual.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            var inDatabase = _tableTypes.FirstOrDefault(e => e.Id == id);
            Assert.Null(inDatabase);
            Assert.Null(actual.Data);
        }
        #endregion Unit Tests

        #region Private member methods
        private ITableTypeRepository SetUpTableTypeRepository()
        {
            var mockTableTypeRepository = new Mock<ITableTypeRepository>();
            mockTableTypeRepository.Setup(m => m.DeleteAsync(It.IsAny<Expression<Func<TableType, bool>>>()))
                .ReturnsAsync(
                (Expression<Func<TableType, bool>> expression)
                =>
                {
                    var inDatabase = _tableTypes.AsQueryable().FirstOrDefault(expression);
                    if (inDatabase is not null)
                    {
                        _tableTypes.Remove(inDatabase);
                    }
                    return true;
                });
            return mockTableTypeRepository.Object;
        }

        private IMapper SetUpMapper()
        {
            var mapperMock = new Mock<IMapper>();
            return mapperMock.Object;
        }
        #endregion

    }
}
