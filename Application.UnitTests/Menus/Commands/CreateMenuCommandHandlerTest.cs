using Application.Menus.Commands;
using Application.Menus.Response;
using Application.Common.Models;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Moq;
using NUnit.Framework;
using System.Net;

namespace Application.UnitTests.Menus.Commands
{
    [TestFixture]
    public class CreateMenuCommandHandlerTest
    {
        private List<Menu> _Menus;
        private IMenuRepository _MenuRepository;
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;

        [SetUp]
        public void ReInitializeTest()
        {
            _Menus = DataSource.Menus;
            _MenuRepository = SetUpMenuRepository();
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(x => x.MenuRepository).Returns(_MenuRepository);
            _unitOfWork = unitOfWork.Object;
            _mapper = SetUpMapper();
        }

        [TearDown]
        public void DisposeTest()
        {
            _MenuRepository = null;
            _unitOfWork = null;
            _Menus = null;
        }

        #region Unit Tests
        [TestCase("abcdef", "test description", false)]
        public async Task Should_Create_Menu(string name, string description, bool available)
        {
            //Arrange
            var request = new CreateMenuCommand()
            {
                Name = name,
                Description = description
            };
            var handler = new CreateMenuCommandHandler(_unitOfWork, _mapper);
            var expected = new Response<MenuDto>(new MenuDto { Id = _Menus.Max(e => e.Id) + 1, Name = name })
            {
                StatusCode = HttpStatusCode.Created
            };
            var count = _Menus.Count + 1;
            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.That(actual.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(_Menus.Count, Is.EqualTo(count));
            var inDatabase = _Menus.FirstOrDefault(e => e.Name.Equals(name));
            Assert.NotNull(inDatabase);
            Assert.That(actual.Data, Is.EqualTo(expected.Data));
        }
        #endregion Unit Tests

        #region Private member methods
        private IMenuRepository SetUpMenuRepository()
        {
            var mockMenuRepository = new Mock<IMenuRepository>();
            mockMenuRepository.Setup(m => m.InsertAsync(It.IsAny<Menu>()))
                .ReturnsAsync(
                (Menu Menu)
                =>
                {
                    Menu.Id = _Menus.Max(e => e.Id) + 1;
                    _Menus.Add(Menu);
                    return Menu;
                });
            return mockMenuRepository.Object;
        }

        private IMapper SetUpMapper()
        {
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<Menu>(It.IsAny<CreateMenuCommand>()))
                .Returns((CreateMenuCommand command) => new Menu
                {
                    Name = command.Name
                });
            mapperMock.Setup(m => m.Map<MenuDto>(It.IsAny<Menu>()))
                .Returns((Menu Menu) => new MenuDto
                {
                    Id = Menu.Id,
                    Name = Menu.Name
                });
            return mapperMock.Object;
        }
        #endregion

    }
}
