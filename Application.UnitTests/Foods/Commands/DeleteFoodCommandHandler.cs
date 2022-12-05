using Application.Foods.Commands;
using Application.Foods.Response;
using Application.Models;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;
using System.Net;

namespace Application.UnitTests.Foods.Commands
{
    [TestFixture]
    public class DeleteFoodCommandHandlerTest
    {
        private List<Food> _foods;
        private IFoodRepository _FoodRepository;
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;
        private IMediator _mediator;

        [SetUp]
        public void ReInitializeTest()
        {
            _foods = DataSource.Foods;
            _FoodRepository = SetUpFoodRepository();
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(x => x.FoodRepository).Returns(_FoodRepository);
            _unitOfWork = unitOfWork.Object;
            _mapper = SetUpMapper();
            _mediator = SetUpMediator();
        }

        [TearDown]
        public void DisposeTest()
        {
            _FoodRepository = null;
            _unitOfWork = null;
            _foods = null;
        }

        #region Unit Tests
        [TestCase(2)]
        public async Task Should_Remove_Food(int id)
        {
            //Arrange
            var request = new DeleteFoodCommand()
            {
                Id = id
            };
            var handler = new DeleteFoodCommandHandler(_unitOfWork, _mapper, _mediator);
            var expected = new Response<FoodDto>()
            {
                StatusCode = HttpStatusCode.NoContent
            };
            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.That(actual.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            var inDatabase = _foods.FirstOrDefault(e => e.Id == id);
            Assert.Null(inDatabase);
            Assert.Null(actual.Data);
        }
        #endregion Unit Tests

        #region Private member methods
        private IFoodRepository SetUpFoodRepository()
        {
            var mockFoodRepository = new Mock<IFoodRepository>();
            mockFoodRepository.Setup(m => m.DeleteAsync(It.IsAny<Expression<Func<Food, bool>>>()))
                .ReturnsAsync(
                (Expression<Func<Food, bool>> expression)
                =>
                {
                    var inDatabase = _foods.AsQueryable().FirstOrDefault(expression);
                    if (inDatabase is not null)
                    {
                        _foods.Remove(inDatabase);
                    }
                    return true;
                });
            return mockFoodRepository.Object;
        }

        private IMapper SetUpMapper()
        {
            var mapperMock = new Mock<IMapper>();
            return mapperMock.Object;
        }

        private IMediator SetUpMediator()
        {
            var mapperMock = new Mock<IMediator>();
            return mapperMock.Object;
        }
        #endregion

    }
}
