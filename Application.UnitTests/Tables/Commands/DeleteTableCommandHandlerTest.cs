using Application.Common.Models;
using Application.Tables.Commands;
using Application.Tables.Response;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;
using System.Net;

namespace Application.UnitTests.Tables.Commands
{
    [TestFixture]
    public class DeleteTableCommandHandlerTest
    {
        private List<Table> _tables;
        private ITableRepository _TableRepository;
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;

        [SetUp]
        public void ReInitializeTest()
        {
            _tables = DataSource.Tables;
            _TableRepository = SetUpTableRepository();
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(x => x.TableRepository).Returns(_TableRepository);
            _unitOfWork = unitOfWork.Object;
            _mapper = SetUpMapper();
        }

        [TearDown]
        public void DisposeTest()
        {
            _TableRepository = null;
            _unitOfWork = null;
            _tables = null;
        }

        #region Unit Tests
        [TestCase(2)]
        public async Task Should_Remove_Table(int id)
        {
            //Arrange
            var request = new DeleteTableCommand()
            {
                Id = id
            };
            var handler = new DeleteTableCommandHandler(_unitOfWork, _mapper);
            var expected = new Response<TableDto>()
            {
                StatusCode = HttpStatusCode.NoContent
            };
            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.That(actual.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            var inDatabase = _tables.FirstOrDefault(e => e.Id == id);
            Assert.Null(inDatabase);
            Assert.Null(actual.Data);
        }
        #endregion Unit Tests

        #region Private member methods
        private ITableRepository SetUpTableRepository()
        {
            var mockTableRepository = new Mock<ITableRepository>();
            mockTableRepository.Setup(m => m.DeleteAsync(It.IsAny<Expression<Func<Table, bool>>>()))
                .ReturnsAsync(
                (Expression<Func<Table, bool>> expression)
                =>
                {
                    var inDatabase = _tables.AsQueryable().FirstOrDefault(expression);
                    if (inDatabase is not null)
                    {
                        _tables.Remove(inDatabase);
                    }
                    return true;
                });
            return mockTableRepository.Object;
        }

        private IMapper SetUpMapper()
        {
            var mapperMock = new Mock<IMapper>();
            return mapperMock.Object;
        }
        #endregion

    }
}
