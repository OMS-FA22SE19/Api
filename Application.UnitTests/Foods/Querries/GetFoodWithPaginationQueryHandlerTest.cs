using Application.Models;
using Application.Foods.Queries;
using Application.Foods.Response;
using Application.CourseTypes.Response;
using AutoMapper;
using Core.Common;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;
using System.Net;
using Application.Types.Response;
using System.Web.Mvc;

namespace Application.UnitTests.Foods.Queries
{
    [TestFixture]
    public class GetFoodWithPaginationQueryHandlerTest
    {
        private List<Food> _Foods;
        private List<CourseType> _CourseTypes;
        private List<Core.Entities.Type> _Types;
        private List<FoodType> _FoodTypes;
        private IFoodRepository _FoodRepository;
        private ITypeRepository _TypeRepository;
        private ICourseTypeRepository _CourseTypeRepository;
        private IFoodTypeRepository _FoodTypeRepository;
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;

        [OneTimeSetUp]
        public void SetUp()
        {
            _Foods = DataSource.Foods;
            _CourseTypes = DataSource.CourseTypes;
            _FoodTypes = DataSource.FoodTypes;
            _Types = DataSource.Types;
            foreach (Food food in _Foods)
            {
                food.CourseType = _CourseTypes.Find(ct => ct.Id == food.CourseTypeId);
                food.FoodTypes = _FoodTypes.FindAll(ft => ft.FoodId == food.Id);
                if (food.FoodTypes != null)
                {
                    foreach (FoodType type in food.FoodTypes)
                    {
                        type.Type = _Types.Find(t => t.Id == type.TypeId);
                    }
                }
            }
        }

        [SetUp]
        public void ReInitializeTest()
        {
            _Foods = DataSource.Foods;
            _CourseTypes = DataSource.CourseTypes;
            _FoodTypes = DataSource.FoodTypes;
            _Types = DataSource.Types;
            foreach (Food food in _Foods)
            {
                food.CourseType = _CourseTypes.Find(ct => ct.Id == food.CourseTypeId);
                food.FoodTypes = _FoodTypes.FindAll(ft => ft.FoodId == food.Id);
                if (food.FoodTypes != null)
                {
                    foreach (FoodType type in food.FoodTypes)
                    {
                        type.Type = _Types.Find(t => t.Id == type.TypeId);
                    }
                }
            }
            _FoodRepository = SetUpFoodRepository();
            _TypeRepository = SetUpTypeRepository();
            _CourseTypeRepository = SetUpCourseTypeRepository();
            _FoodTypeRepository = SetUpFoodTypeRepository();
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(x => x.FoodRepository).Returns(_FoodRepository);
            unitOfWork.SetupGet(x => x.CourseTypeRepository).Returns(_CourseTypeRepository);
            unitOfWork.SetupGet(x => x.TypeRepository).Returns(_TypeRepository);
            unitOfWork.SetupGet(x => x.FoodTypeRepository).Returns(_FoodTypeRepository);
            _unitOfWork = unitOfWork.Object;
            _mapper = SetUpMapper();
        }

        [TearDown]
        public void DisposeTest()
        {
            _FoodRepository = null;
            _TypeRepository = null;
            _CourseTypeRepository = null;
            _FoodTypeRepository = null;
            _unitOfWork = null;
        }

        [OneTimeTearDown]
        public void DisposeAllObjects()
        {
            _Foods = null;
            _CourseTypes = null;
            _FoodTypes = null;
            _Types = null;
        }

        #region Unit Tests

        [TestCase]
        [TestCase(1, 50, FoodProperty.Name, "", null, false)]
        [TestCase(1, 2, FoodProperty.Name, "", null, false)]
        [TestCase(2, 2, FoodProperty.Name, "", null, false)]
        [TestCase(1, 50, FoodProperty.Name, "", FoodProperty.Name, false)]
        [TestCase(1, 50, FoodProperty.Name, "", FoodProperty.Name, true)]
        [TestCase(1, 50, FoodProperty.Name, "", FoodProperty.Description, false)]
        [TestCase(1, 50, FoodProperty.Name, "", FoodProperty.Description, true)]
        [TestCase(1, 50, FoodProperty.Name, "", FoodProperty.Available, false)]
        [TestCase(1, 50, FoodProperty.Name, "", FoodProperty.Available, true)]
        [TestCase(1, 50, FoodProperty.Name, "random", null, false)]
        [TestCase(1, 50, FoodProperty.CourseType, "4", null, false)]
        public async Task Should_Return_With_Condition(
            int pageIndex = 1,
            int pageSize = 50,
            FoodProperty SearchBy = FoodProperty.Name,
            string searchValue = "",
            FoodProperty? orderBy = null,
            bool IsDescending = false)
        {
            //Arrange
            var request = new GetFoodWithPaginationQuery()
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                SearchBy = SearchBy,
                SearchValue = searchValue,
                OrderBy = orderBy,
                IsDescending = IsDescending
            };
            var handler = new GetFoodWithPaginationQueryHandler(_unitOfWork, _mapper);
            var conditionedList = _Foods;
            //if (!string.IsNullOrWhiteSpace(searchValue))
            //{
            //    conditionedList = conditionedList.Where(e => e.Ingredient.Contains(request.SearchValue)
            //    || request.SearchValue.Contains(e.Id.ToString())
            //    || e.Name.ToString().Contains(request.SearchValue)).ToList();
            //}

            if (!string.IsNullOrWhiteSpace(request.SearchValue))
            {
                switch (request.SearchBy)
                {
                    case FoodProperty.Name:
                        conditionedList = conditionedList.Where(e => e.Name.Contains(request.SearchValue)).ToList();
                        break;
                    case FoodProperty.Description:
                        conditionedList = conditionedList.Where(e => e.Description.Contains(request.SearchValue)).ToList();
                        break;
                    case FoodProperty.Ingredient:
                        conditionedList = conditionedList.Where(e => e.Ingredient.Contains(request.SearchValue)).ToList();
                        break;
                    case FoodProperty.Available:
                        break;
                    case FoodProperty.CourseType:
                        List<Expression<Func<CourseType, bool>>> courseTypeFilters = new();
                        //courseTypeFilters.Add(e => e.Name.Contains(request.SearchValue) && !e.IsDeleted);
                        //var courseTypes = await _unitOfWork.CourseTypeRepository.GetAllAsync(courseTypeFilters, null, null);
                        //var courseTypeIds = courseTypes.Select(e => e.Id).ToList();
                        var courseTypes = _CourseTypes.FindAll(e => e.Name.Contains(request.SearchValue) && !e.IsDeleted);
                        var courseTypeIds = courseTypes.Select(e => e.Id).ToList();
                        conditionedList = conditionedList.Where(e => courseTypeIds.Contains(e.CourseTypeId)).ToList();
                        break;
                    default:
                        break;
                }
            }

            conditionedList = conditionedList.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            switch (orderBy)
            {
                case (FoodProperty.Name):
                    if (request.IsDescending)
                    {
                        conditionedList = conditionedList.OrderByDescending(x => x.Name).ToList();
                        break;
                    }
                    conditionedList = conditionedList.OrderBy(x => x.Name).ToList();
                    break;
                case (FoodProperty.Description):
                    if (request.IsDescending)
                    {
                        conditionedList = conditionedList.OrderByDescending(x => x.Description).ToList();
                        break;
                    }
                    conditionedList = conditionedList.OrderBy(x => x.Description).ToList();
                    break;
                case (FoodProperty.Available):
                    if (request.IsDescending)
                    {
                        conditionedList = conditionedList.OrderByDescending(x => x.Available).ToList();
                        break;
                    }
                    conditionedList = conditionedList.OrderBy(x => x.Available).ToList();
                    break;
                default:
                    break;
            }
            var expectedList = new List<FoodDto>();
            foreach (var food in conditionedList)
            {
                var CourseType = _CourseTypes.Find(ct => ct.Id == food.CourseTypeId);
                var mappedCourseType = _mapper.Map<CourseTypeDto>(CourseType);

                var mappedType = new List<TypeDto>();

                var FoodTypes = _FoodTypes.FindAll(ft => ft.FoodId == food.Id);
                if (FoodTypes.Any())
                {
                    foreach (FoodType foodType in FoodTypes)
                    {
                        var type = _Types.Find(t => t.Id == foodType.TypeId);
                        mappedType.Add(_mapper.Map<TypeDto>(type));
                    }
                }

                expectedList.Add(new FoodDto
                {
                    Id = food.Id,
                    Name = food.Name,
                    Description = food.Description,
                    Ingredient= food.Ingredient,
                    Available= food.Available,
                    CourseType= mappedCourseType,
                    PictureUrl= food.PictureUrl,
                    Types= mappedType, 
                    IsDeleted = true
                });
            }

            var expected = new Response<PaginatedList<FoodDto>>(new PaginatedList<FoodDto>(expectedList, _Foods.Count, pageIndex, pageSize));
            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.That(actual.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(actual.Data, Is.Not.Null);
            Assert.That(actual, Is.TypeOf(expected.GetType()));
            Assert.That(expected.Data.Count, Is.EqualTo(actual.Data.Count));
            Assert.IsTrue(actual.Data.SequenceEqual(expected.Data, new FoodDtoComparer()));
        }
        #endregion Unit Tests

        #region Private member methods
        private IFoodRepository SetUpFoodRepository()
        {
            var mockFoodRepository = new Mock<IFoodRepository>();
            mockFoodRepository
                .Setup(m => m.GetPaginatedListAsync(It.IsAny<List<Expression<Func<Food, bool>>>>()
                    , It.IsAny<Func<IQueryable<Food>, IOrderedQueryable<Food>>>()
                    , It.IsAny<string>()
                    , It.IsAny<int>()
                    , It.IsAny<int>()))
                .ReturnsAsync(
                (List<Expression<Func<Food, bool>>> filters,
                    Func<IQueryable<Food>, IOrderedQueryable<Food>> orderBy,
                    string includeProperties,
                    int pageIndex,
                    int pageSize)
                =>
                {
                    var query = _Foods.AsQueryable();

                    if (filters is not null)
                    {
                        foreach (var filter in filters)
                        {
                            query = query.Where(filter);
                        }
                    }

                    query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);

                    return orderBy is not null
                            ? new PaginatedList<Food>(orderBy(query).ToList(), query.Count(), pageIndex, pageSize)
                            : new PaginatedList<Food>(query.ToList(), query.Count(), pageIndex, pageSize);
                });
            return mockFoodRepository.Object;
        }

        private ICourseTypeRepository SetUpCourseTypeRepository()
        {
            var mockCourseTypeRepository = new Mock<ICourseTypeRepository>();
            mockCourseTypeRepository.Setup(m => m.GetAsync(It.IsAny<Expression<Func<CourseType, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(
                (Expression<Func<CourseType, bool>> filter,
                string includeString)
                =>
                {
                    return _CourseTypes.AsQueryable().FirstOrDefault(filter);
                });
            mockCourseTypeRepository.Setup(m => m.GetAllAsync(It.IsAny<List<Expression<Func<CourseType, bool>>>>()
                , It.IsAny<Func<IQueryable<CourseType>, IOrderedQueryable<CourseType>>>()
                , It.IsAny<string>()))
                .ReturnsAsync(
                (List<Expression<Func<CourseType, bool>>> filters,
                Func<IQueryable<CourseType>, IOrderedQueryable<CourseType>> orderBy,
                string includeString)
                =>
                {
                    var query = _CourseTypes.AsQueryable();

                    if (filters is not null)
                    {
                        foreach (var filter in filters)
                        {
                            query = query.Where(filter);
                        }
                    }
                    return query.ToList();
                });
            return mockCourseTypeRepository.Object;
        }

        private ITypeRepository SetUpTypeRepository()
        {
            var mockTypeRepository = new Mock<ITypeRepository>();
            mockTypeRepository.Setup(m => m.GetAsync(It.IsAny<Expression<Func<Core.Entities.Type, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(
                (Expression<Func<Core.Entities.Type, bool>> filter,
                string includeString)
                =>
                {
                    return _Types.AsQueryable().FirstOrDefault(filter);
                });
            return mockTypeRepository.Object;
        }

        private IFoodTypeRepository SetUpFoodTypeRepository()
        {
            var mockFoodTypeRepository = new Mock<IFoodTypeRepository>();
            mockFoodTypeRepository.Setup(m => m.GetAsync(It.IsAny<Expression<Func<FoodType, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(
                (Expression<Func<FoodType, bool>> filter,
                string includeString)
                =>
                {
                    return _FoodTypes.AsQueryable().FirstOrDefault(filter);
                });
            return mockFoodTypeRepository.Object;
        }

        private IMapper SetUpMapper()
        {
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<PaginatedList<Food>, PaginatedList<FoodDto>>(It.IsAny<PaginatedList<Food>>()))
                .Returns((PaginatedList<Food> entities) =>
                {
                    var dtos = new List<FoodDto>();
                    foreach (var entity in entities)
                    {
                        dtos.Add(new FoodDto
                        {
                            Id = entity.Id,
                            Name = entity.Name,
                            Description = entity.Description,
                            Ingredient = entity.Ingredient,
                            Available = entity.Available,
                            PictureUrl = entity.PictureUrl,
                            IsDeleted = entity.Available,
                            CourseType = _mapper.Map<CourseTypeDto>(entity.CourseType),
                            Types = _mapper.Map<List<TypeDto>>(entity.FoodTypes)
                        });
                    }
                    return new PaginatedList<FoodDto>(entities.PageNumber, entities.TotalPages, entities.TotalPages, dtos);
                });
            mapperMock.Setup(m => m.Map<CourseTypeDto>(It.IsAny<CourseType>()))
                .Returns((CourseType courseType) => new CourseTypeDto
                {
                    Id = courseType.Id,
                    Name = courseType.Name,
                    Description = courseType.Description,
                    IsDeleted = courseType.IsDeleted
                });
            mapperMock.Setup(m => m.Map<TypeDto>(It.IsAny<Core.Entities.Type>()))
                .Returns((Core.Entities.Type type) => new TypeDto
                {
                    Id = type.Id,
                    Name = type.Name,
                    Description = type.Description,
                    IsDeleted = type.IsDeleted
                });
            return mapperMock.Object;
        }
        #endregion
    }
}
