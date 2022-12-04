using Application.Common.Exceptions;
using Application.Menus.Commands;
using Application.Menus.Response;
using Application.Models;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;
using System.Net;

namespace Application.UnitTests.Menus.Commands
{
    public class UpdateMenuCommandHandlerTest
    {
        private List<Menu> _menu;
        private IMenuRepository _courseTypeRepository;
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;

        [SetUp]
        public void ReInitializeTest()
        {
            _menu = DataSource.Menus;
            _courseTypeRepository = SetUpMenuRepository();
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(x => x.MenuRepository).Returns(_courseTypeRepository);
            _unitOfWork = unitOfWork.Object;
            _mapper = SetUpMapper();
        }

        [TearDown]
        public void DisposeTest()
        {
            _courseTypeRepository = null;
            _unitOfWork = null;
            _menu = null;
        }

        #region Unit Tests
        [TestCase(2, "abcdef", "Test Update", false)]
        [TestCase(1, "abcdef", "random name", true)]
        [TestCase(1, "random name", "Test Update", false)]
        [TestCase(2, "abcdef", "random name", false)]
        public async Task Should_Update_Menu(int id, string name, string description, bool available)
        {
            //Arrange
            var request = new UpdateMenuCommand()
            {
                Id = id,
                Name = name,
                Description = description,
                Available = available
            };
            var handler = new UpdateMenuCommandHandler(_unitOfWork, _mapper);
            var expected = new Menu()
            {
                Id = id,
                Name = name,
                Description = description,
                Available = available
            };
            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.That(actual.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(actual, Is.TypeOf(typeof(Response<MenuDto>)));
            Assert.Null(actual.Data);
            var inDatabase = _menu.FirstOrDefault(e => e.Id == id);
            Assert.That(inDatabase, Is.EqualTo(expected));
        }
        [TestCase(10)]
        [TestCase(3)]
        public async Task Should_Return_Throw_NotFound_Exception(int id)
        {
            //Arrange
            var request = new UpdateMenuCommand()
            {
                Id = id
            };
            var handler = new UpdateMenuCommandHandler(_unitOfWork, _mapper);

            //Act
            var ex = Assert.ThrowsAsync<NotFoundException>(async () => await handler.Handle(request, CancellationToken.None));

            //Assert
            Assert.That(ex.Message, Is.EqualTo($"Entity {nameof(Menu)} ({request.Id}) was not found."));
        }
        #endregion Unit Tests

        #region Private member methods
        private IMenuRepository SetUpMenuRepository()
        {
            var mockMenuRepository = new Mock<IMenuRepository>();
            mockMenuRepository.Setup(m => m.GetAsync(It.IsAny<Expression<Func<Menu, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(
                (Expression<Func<Menu, bool>> expression,
                string includeProperties)
                =>
                {
                    return _menu.AsQueryable().FirstOrDefault(expression);
                });
            mockMenuRepository.Setup(m => m.UpdateAsync(It.IsAny<Menu>()))
                .ReturnsAsync(
                (Menu updatedEntity)
                =>
                {
                    var inDatabase = _menu.FirstOrDefault(e => e.Id == updatedEntity.Id);
                    inDatabase.Name = updatedEntity.Name;
                    inDatabase.Description = updatedEntity.Description;
                    inDatabase.Available = updatedEntity.Available;
                    return inDatabase;
                });
            return mockMenuRepository.Object;
        }

        private IMapper SetUpMapper()
        {
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<Menu>(It.IsAny<UpdateMenuCommand>()))
                .Returns((UpdateMenuCommand command) => new Menu
                {
                    Id = command.Id,
                    Name = command.Name,
                    Description = command.Description,
                    Available = command.Available
                });
            return mapperMock.Object;
        }
        #endregion
    }
}
