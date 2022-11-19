using Application.Menus.Queries;
using Application.Menus.Response;
using Application.Common.Models;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;
using System.Net;

namespace Application.UnitTests.Menus.Queries
{
    [TestFixture]
    public class GetAvailableQueryHandlerTest
    {
        private List<Menu> _menus;
        private IMenuRepository _menuRepository;
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;

        [OneTimeSetUp]
        public void SetUp()
        {
            _menus = DataSource.Menus;
        }

        [SetUp]
        public void ReInitializeTest()
        {
            _menuRepository = SetUpMenuRepository();
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(x => x.MenuRepository).Returns(_menuRepository);
            _unitOfWork = unitOfWork.Object;
            _mapper = SetUpMapper();
        }

        [TearDown]
        public void DisposeTest()
        {
            _menuRepository = null;
            _unitOfWork = null;
        }

        [OneTimeTearDown]
        public void DisposeAllObjects()
        {
            _menus = null;
        }

        #region Unit Tests
        [TestCase]
        public async Task Should_Return_Menu()
        {
            //Arrange
            var request = new GetAvailableMenuQuery()
            {
            };
            var handler = new GetAvailableMenuQueryHandler(_unitOfWork, _mapper);

            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            var inDatabase = _menus.FirstOrDefault(x => !x.Available && !x.IsDeleted);
            Assert.NotNull(inDatabase);
            var expected = new Response<MenuDto>(new MenuDto
            {
                Id = inDatabase.Id,
                Name = inDatabase.Name,
                Description = inDatabase.Description,
                Available = inDatabase.Available
            });
            Assert.That(actual.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(expected.Data, Is.EqualTo(actual.Data));
        }
        #endregion Unit Tests

        #region Private member methods
        private IMenuRepository SetUpMenuRepository()
        {
            var mockMenuRepository = new Mock<IMenuRepository>();
            mockMenuRepository.Setup(m => m.GetAsync(It.IsAny<Expression<Func<Menu, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(
                (Expression<Func<Menu, bool>> filter,
                string includeString)
                =>
                {
                    return _menus.AsQueryable().FirstOrDefault(filter);
                });
            return mockMenuRepository.Object;
        }

        private IMapper SetUpMapper()
        {
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<MenuDto>(It.IsAny<Menu>()))
                .Returns((Menu menu) => new MenuDto
                {
                    Id = menu.Id,
                    Name = menu.Name,
                    Description = menu.Description,
                    Available = menu.Available
                });
            return mapperMock.Object;
        }
        #endregion

    }
}
