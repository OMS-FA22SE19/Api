using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.CourseTypes.Response;
using Application.Foods.Commands;
using Application.Foods.Queries;
using Application.Foods.Response;
using Application.Models;
using Application.TableTypes.Response;
using Application.Types.Response;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;
using System.Net;

namespace Application.UnitTests.Foods.Queries
{
    [TestFixture]
    public class GetFoodWithIdQueryHandlerTest
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
                    foreach(FoodType type in food.FoodTypes)
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
        [TestCase(1)]
        public async Task Should_Return_Food(int id)
        {
            //Arrange
            var request = new GetFoodWithIdQuery()
            {
                Id = id
            };
            var handler = new GetFoodWithIdQueryHandler(_unitOfWork, _mapper);

            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            var inDatabase = _Foods.FirstOrDefault(x => x.Id == id);
            Assert.NotNull(inDatabase);
            var CourseType = _CourseTypes.Find(ct => ct.Id == inDatabase.CourseTypeId);
            var mappedCourseType = _mapper.Map<CourseTypeDto>(CourseType);

            var mappedType = new List<TypeDto>();
            
            var FoodTypes = _FoodTypes.FindAll(ft => ft.FoodId == id);
            if(FoodTypes.Any())
            {
                foreach (FoodType foodType in FoodTypes)
                {
                    var type = _Types.Find(t => t.Id == foodType.TypeId);
                    mappedType.Add(_mapper.Map<TypeDto>(type));
                }
            }

            var expected = new Response<FoodDto>(new FoodDto
            {
                Id = inDatabase.Id,
                Name = inDatabase.Name,
                Description = inDatabase.Description,
                Ingredient= inDatabase.Ingredient,
                PictureUrl= inDatabase.PictureUrl,
                IsDeleted = true,
                Available = inDatabase.Available,
                CourseType = mappedCourseType,
                Types = mappedType
            });

            Assert.That(actual.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(expected.Data, Is.EqualTo(actual.Data));
        }

        [TestCase(10)]
        public async Task Should_Return_Throw_NotFound_Exception(int id)
        {
            //Arrange
            var request = new GetFoodWithIdQuery()
            {
                Id = id
            };
            var handler = new GetFoodWithIdQueryHandler(_unitOfWork, _mapper);

            //Act
            var ex = Assert.ThrowsAsync<NotFoundException>(async () => await handler.Handle(request, CancellationToken.None));

            //Assert
            Assert.That(ex.Message, Is.EqualTo($"Entity {nameof(Food)} ({request.Id}) was not found."));
        }
        #endregion Unit Tests

        #region Private member methods
        private IFoodRepository SetUpFoodRepository()
        {
            var mockFoodRepository = new Mock<IFoodRepository>();
            mockFoodRepository.Setup(m => m.GetAsync(It.IsAny<Expression<Func<Food, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(
                (Expression<Func<Food, bool>> filter,
                string includeString)
                =>
                {
                    return _Foods.AsQueryable().FirstOrDefault(filter);
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

        private IUploadService SetUpUploadService()
        {
            var uploadMock = new Mock<IUploadService>();
            uploadMock.Setup(m => m.UploadAsync(It.IsAny<IFormFile>(), It.IsAny<string>())).Returns(Task.FromResult("NewPicture"));
            return uploadMock.Object;
        }

        private IMapper SetUpMapper()
        {
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<FoodDto>(It.IsAny<Food>()))
                .Returns((Food Food) => new FoodDto
                {
                    Id = Food.Id,
                    Name = Food.Name,
                    Description = Food.Description,
                    Ingredient = Food.Ingredient,
                    Available = Food.Available,
                    PictureUrl = Food.PictureUrl,
                    IsDeleted = Food.Available,
                    CourseType = _mapper.Map<CourseTypeDto>(Food.CourseType),
                    Types = _mapper.Map<List<TypeDto>>(Food.FoodTypes)
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
