using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.CourseTypes.Response;
using Application.Foods.Commands;
using Application.Foods.Response;
using Application.Models;
using Application.Types.Response;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;
using System.Net;
using System.Text;

namespace Application.UnitTests.Foods.Commands
{
    public class UpdateFoodCommandHandlerTest
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
        private IUploadService _uploadService;

        [SetUp]
        public void ReInitializeTest()
        {
            _Foods = DataSource.Foods;
            _CourseTypes = DataSource.CourseTypes;
            _FoodTypes = DataSource.FoodTypes;
            _Types = DataSource.Types;
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
            _uploadService = SetUpUploadService();
        }

        [TearDown]
        public void DisposeTest()
        {
            _FoodRepository = null;
            _CourseTypeRepository = null;
            _unitOfWork = null;
            _Foods = null;
        }

        #region Unit Tests
        [TestCase(2, "abcdef", "Test Update", "test food", false, 1, new[] { 1, 2 })]
        [TestCase(2, "abcdef", "Test Update", "test food", false, 1, null)]
        public async Task Should_Update_Food(int id, string name, string description, string ingredient, bool available, int courseTypeId, int[]? Types)
        {
            var bytes = Encoding.UTF8.GetBytes("This is a dummy file");
            IFormFile file = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "Data", "dummy.txt");

            //List<int> types = Types.ToList();
            //Arrange
            var request = new UpdateFoodCommand()
            {
                Id = id,
                Name = name,
                Description = description,
                Ingredient = ingredient,
                Available = available,
                Picture = file,
                CourseTypeId = courseTypeId,
                Types = Types
            };
            var handler = new UpdateFoodCommandHandler(_unitOfWork, _mapper, _uploadService);

            var foodType = new List<FoodType>();
            if (Types is not null)
            {
                foreach (var type in Types)
                {
                    foodType.Add(new FoodType { FoodId = id, TypeId = type});
                }
            }
            
            
            var expected = new Food()
            {
                Id = id,
                Name = name,
                Description = description,
                Ingredient = ingredient,
                Available = available,
                PictureUrl = "NewPicture",
                CourseTypeId = courseTypeId,
                FoodTypes = foodType
            };
            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.That(actual.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(actual, Is.TypeOf(typeof(Response<FoodDto>)));
            Assert.Null(actual.Data);
            var inDatabase = _Foods.FirstOrDefault(e => e.Id == id);
            Assert.That(inDatabase, Is.EqualTo(expected));
        }
        [TestCase(10)]
        public async Task Should_Return_Throw_NotFound_Exception(int id)
        {
            //Arrange
            var request = new UpdateFoodCommand()
            {
                Id = id
            };
            var handler = new UpdateFoodCommandHandler(_unitOfWork, _mapper, _uploadService);

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
                (Expression<Func<Food, bool>> expression,
                string includeProperties)
                =>
                {
                    return _Foods.AsQueryable().FirstOrDefault(expression);
                });
            mockFoodRepository.Setup(m => m.UpdateAsync(It.IsAny<Food>()))
                .ReturnsAsync(
                (Food updatedEntity)
                =>
                {
                    var inDatabase = _Foods.FirstOrDefault(e => e.Id == updatedEntity.Id);
                    inDatabase.Name = updatedEntity.Name;
                    inDatabase.Description = updatedEntity.Description;
                    inDatabase.Ingredient = updatedEntity.Ingredient;
                    inDatabase.PictureUrl = "NewPicture";
                    inDatabase.CourseTypeId = updatedEntity.CourseTypeId;
                    inDatabase.Available = updatedEntity.Available;
                    return inDatabase;
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
            mapperMock.Setup(m => m.Map<Food>(It.IsAny<UpdateFoodCommand>()))
                .Returns((UpdateFoodCommand command) => new Food
                {
                    Name = command.Name,
                    Description = command.Description,
                    Ingredient = command.Ingredient,
                    Available = command.Available,
                    PictureUrl = "NewPicture",
                    IsDeleted = false,
                    CourseTypeId = command.CourseTypeId
                });
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
            return mapperMock.Object;
        }
        #endregion

    }
}
