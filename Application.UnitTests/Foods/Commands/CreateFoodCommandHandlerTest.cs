using Application.Common.Exceptions;
using Application.Models;
using Application.Foods.Commands;
using Application.Foods.Response;
using AutoMapper;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;
using System.Net;
using Application.CourseTypes.Response;
using Application.Types.Response;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace Application.UnitTests.Foods.Commands
{
    [TestFixture]
    public class CreateFoodCommandHandlerTest
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
        [TestCase("Random food", "test food", "egg", 1, new []{1, 2})]
        public async Task Should_Create_Food(string name, string description, string ingredient, int courseTypeId, int[] Types)
        {
            var bytes = Encoding.UTF8.GetBytes("This is a dummy file");
            IFormFile file = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "Data", "dummy.txt");

            List<int> types = Types.ToList();
            //Arrange
            var request = new CreateFoodCommand()
            {
                Name = name,
                Description = description,
                Ingredient = ingredient,
                Available = true,
                Picture = file,
                CourseTypeId = courseTypeId,
                Types = Types
            };
            var handler = new CreateFoodCommandHandler(_unitOfWork, _mapper, _uploadService);
            var CourseType = _CourseTypes.Find(ct => ct.Id == courseTypeId);
            var mappedCourseType = _mapper.Map<CourseTypeDto>(CourseType);
            var expected = new Response<FoodDto>(new FoodDto
            {
                Id = _Foods.Max(e => e.Id) + 1,
                Name = name,
                Description = description,
                Ingredient = ingredient,
                Available = true,
                IsDeleted= true,
                PictureUrl = "NewPicture",
                CourseType = mappedCourseType,
                //Types = Types
            })
            {
                StatusCode = HttpStatusCode.Created
            };
            var count = _Foods.Count + 1;
            //Act
            var actual = await handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.That(actual.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(_Foods.Count, Is.EqualTo(count));
            var inDatabase = _Foods.FirstOrDefault(e => e.Name.Equals(name));
            Assert.NotNull(inDatabase);
            Assert.That(actual.Data, Is.EqualTo(expected.Data));
        }

        [TestCase("Random food", "test food", "egg", 10, new[] { 1, 2 })]
        public async Task Should_Return_Throw_NotFound_Exception(string name, string description, string ingredient, int courseTypeId, int[] Types)
        {
            var bytes = Encoding.UTF8.GetBytes("This is a dummy file");
            IFormFile file = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "Data", "dummy.txt");

            //Arrange
            var request = new CreateFoodCommand()
            {
                Name = name,
                Description = description,
                Ingredient = ingredient,
                Available = true,
                Picture = file,
                CourseTypeId = courseTypeId,
                Types = Types
            };
            var handler = new CreateFoodCommandHandler(_unitOfWork, _mapper, _uploadService);

            //Act
            var ex = Assert.ThrowsAsync<NotFoundException>(async () => await handler.Handle(request, CancellationToken.None));

            //Assert
            Assert.That(ex.Message, Is.EqualTo($"Entity {nameof(CourseType)} ({request.CourseTypeId}) was not found."));
        }
        #endregion Unit Tests

        #region Private member methods
        private IFoodRepository SetUpFoodRepository()
        {
            var mockFoodRepository = new Mock<IFoodRepository>();
            mockFoodRepository.Setup(m => m.InsertAsync(It.IsAny<Food>()))
                .ReturnsAsync(
                (Food Food)
                =>
                {
                    Food.Id = _Foods.Max(e => e.Id) + 1;
                    _Foods.Add(Food);
                    return Food;
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
            mapperMock.Setup(m => m.Map<Food>(It.IsAny<CreateFoodCommand>()))
                .Returns((CreateFoodCommand command) => new Food
                {
                    Name= command.Name,
                    Description= command.Description,
                    Ingredient= command.Ingredient,
                    Available= command.Available,
                    PictureUrl= "NewPicture",
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
