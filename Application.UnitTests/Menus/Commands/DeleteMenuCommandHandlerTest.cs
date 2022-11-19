using Application.Menus.Commands;
using Application.Menus.Response;
using Application.Common.Models;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;
using System.Net;

namespace Application.UnitTests.Menus.Commands
{
    [TestFixture]
    public class DeleteMenuCommandHandlerTest
    {
        private List<Menu> _menus;
        private IMenuRepository _MenuRepository;
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;
        private IMediator _mediator;

        [SetUp]
        public void ReInitializeTest()
        {
            _menus = DataSource.Menus;
            _MenuRepository = SetUpMenuRepository();
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(x => x.MenuRepository).Returns(_MenuRepository);
            _unitOfWork = unitOfWork.Object;
            _mapper = SetUpMapper();
            _mediator = SetUpMediator();
        }

        [TearDown]
        public void DisposeTest()
        {
            _MenuRepository = null;
            _unitOfWork = null;
            _menus = null;
        }

        #region Unit Tests
        [TestCase(2)]
        public async Task Should_Remove_Menu(int id)
        {
            //Arrange
            var request = new DeleteMenuCommand()
            {
                Id = id
            };
            var handler = new DeleteMenuCommandHandler(_unitOfWork, _mapper, _mediator);
            var expected = new Response<MenuDto>()
            {
                StatusCode = HttpStatusCode.NoContent
            };
            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.That(actual.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            var inDatabase = _menus.FirstOrDefault(e => e.Id == id);
            Assert.Null(inDatabase);
            Assert.Null(actual.Data);
        }
        #endregion Unit Tests

        #region Private member methods
        private IMenuRepository SetUpMenuRepository()
        {
            var mockMenuRepository = new Mock<IMenuRepository>();
            mockMenuRepository.Setup(m => m.DeleteAsync(It.IsAny<Expression<Func<Menu, bool>>>()))
                .ReturnsAsync(
                (Expression<Func<Menu, bool>> expression)
                =>
                {
                    var inDatabase = _menus.AsQueryable().FirstOrDefault(expression);
                    if (inDatabase is not null)
                    {
                        _menus.Remove(inDatabase);
                    }
                    return true;
                });
            return mockMenuRepository.Object;
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
