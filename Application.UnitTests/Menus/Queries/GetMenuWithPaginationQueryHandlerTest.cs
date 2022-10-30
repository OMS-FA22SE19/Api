using Application.Menus.Queries;
using Application.Menus.Response;
using Application.Models;
using AutoMapper;
using Core.Common;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;
using System.Net;

namespace Application.UnitTests.Menus.Queries
{
    [TestFixture]
    public class GetMenuWithPaginationQueryHandlerTest
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
        [TestCase(1, 50, "", null, false)]
        [TestCase(1, 2, "", null, false)]
        [TestCase(2, 2, "", null, false)]
        [TestCase(1, 50, "", MenuProperty.Name, false)]
        [TestCase(1, 50, "", MenuProperty.Name, true)]
        [TestCase(1, 50, "", MenuProperty.Description, false)]
        [TestCase(1, 50, "", MenuProperty.Description, true)]
        [TestCase(1, 50, "", MenuProperty.IsHidden, false)]
        [TestCase(1, 50, "", MenuProperty.IsHidden, true)]
        [TestCase(1, 50, "2", null, false)]
        [TestCase(1, 50, "Main", null, false)]
        public async Task Should_Return_With_Condition(
            int pageIndex = 1,
            int pageSize = 50,
            string searchValue = "",
            MenuProperty? orderBy = null,
            bool IsDescending = false)
        {
            //Arrange
            var request = new GetMenuWithPaginationQuery()
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                SearchValue = searchValue,
                OrderBy = orderBy,
                IsDescending = IsDescending
            };
            var handler = new GetMenuWithPaginationQueryHandler(_unitOfWork, _mapper);
            var conditionedList = _menus;
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                conditionedList = conditionedList.Where(e => e.Name.Contains(request.SearchValue)
                || e.Description.Contains(request.SearchValue)
                || request.SearchValue.Contains(e.Id.ToString())).ToList();
            }

            if (request.IsHidden != null)
            {
                conditionedList = conditionedList.Where(e => e.IsHidden == request.IsHidden).ToList();
            }

            conditionedList = conditionedList.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            switch (orderBy)
            {
                case (MenuProperty.Name):
                    if (request.IsDescending)
                    {
                        conditionedList = conditionedList.OrderByDescending(x => x.Name).ToList();
                        break;
                    }
                    conditionedList = conditionedList.OrderBy(x => x.Name).ToList();
                    break;
                case (MenuProperty.Description):
                    if (request.IsDescending)
                    {
                        conditionedList = conditionedList.OrderByDescending(x => x.Description).ToList();
                        break;
                    }
                    conditionedList = conditionedList.OrderBy(x => x.Description).ToList();
                    break;
                case (MenuProperty.IsHidden):
                    if (request.IsDescending)
                    {
                        conditionedList = conditionedList.OrderByDescending(x => x.IsHidden).ToList();
                        break;
                    }
                    conditionedList = conditionedList.OrderBy(x => x.IsHidden).ToList();
                    break;
                default:
                    break;
            }
            var expectedList = new List<MenuDto>();
            foreach (var menuDto in conditionedList)
            {
                expectedList.Add(new MenuDto
                {
                    Id = menuDto.Id,
                    Name = menuDto.Name,
                    Description = menuDto.Description,
                    IsHidden = menuDto.IsHidden,
                    IsDeleted = menuDto.IsDeleted
                });
            }

            var expected = new Response<PaginatedList<MenuDto>>(new PaginatedList<MenuDto>(expectedList, _menus.Count, pageIndex, pageSize));
            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.That(actual.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(actual.Data, Is.Not.Null);
            Assert.That(actual, Is.TypeOf(expected.GetType()));
            Assert.That(expected.Data.Count, Is.EqualTo(actual.Data.Count));
            Assert.IsTrue(actual.Data.SequenceEqual(expected.Data, new MenuDtoComparer()));
        }
        #endregion Unit Tests

        #region Private member methods
        private IMenuRepository SetUpMenuRepository()
        {
            var mockMenuRepository = new Mock<IMenuRepository>();
            mockMenuRepository
                .Setup(m => m.GetPaginatedListAsync(It.IsAny<List<Expression<Func<Menu, bool>>>>()
                    , It.IsAny<Func<IQueryable<Menu>, IOrderedQueryable<Menu>>>()
                    , It.IsAny<string>()
                    , It.IsAny<int>()
                    , It.IsAny<int>()))
                .ReturnsAsync(
                (List<Expression<Func<Menu, bool>>> filters,
                    Func<IQueryable<Menu>, IOrderedQueryable<Menu>> orderBy,
                    string includeProperties,
                    int pageIndex,
                    int pageSize)
                =>
                {
                    var query = _menus.AsQueryable();

                    if (filters is not null)
                    {
                        foreach (var filter in filters)
                        {
                            query = query.Where(filter);
                        }
                    }

                    query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);

                    return orderBy is not null
                            ? new PaginatedList<Menu>(orderBy(query).ToList(), query.Count(), pageIndex, pageSize)
                            : new PaginatedList<Menu>(query.ToList(), query.Count(), pageIndex, pageSize);
                });
            return mockMenuRepository.Object;
        }

        private IMapper SetUpMapper()
        {
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<PaginatedList<Menu>, PaginatedList<MenuDto>>(It.IsAny<PaginatedList<Menu>>()))
                .Returns((PaginatedList<Menu> entities) =>
                {
                    var dtos = new List<MenuDto>();
                    foreach (var entity in entities)
                    {
                        dtos.Add(new MenuDto
                        {
                            Id = entity.Id,
                            Name = entity.Name,
                            Description = entity.Description,
                            IsHidden = entity.IsHidden,
                            IsDeleted = entity.IsDeleted
                        });
                    }
                    return new PaginatedList<MenuDto>(entities.PageNumber, entities.TotalPages, entities.TotalPages, dtos);
                });
            return mapperMock.Object;
        }
        #endregion
    }
}
