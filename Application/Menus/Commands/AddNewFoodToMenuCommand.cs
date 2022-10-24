using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Mappings;
using Application.Foods.Events;
using Application.Menus.Events;
using Application.Menus.Response;
using Application.Models;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Application.Menus.Commands
{
    public sealed class AddNewFoodToMenuCommand : IMapFrom<Food>, IRequest<Response<MenuDto>>
    {
        public int Id { get; set; }
        [Required]
        [StringLength(1000, MinimumLength = 2)]
        public string Name { get; set; }
        [Required]
        [StringLength(4000, MinimumLength = 2)]
        public string Description { get; set; }
        [Required]
        [StringLength(2000, MinimumLength = 2)]
        public string Ingredient { get; set; }
        public bool Available { get; set; } = true;
        [Required]
        public IFormFile Picture { get; set; }
        public int CourseTypeId { get; set; }

        public IList<int>? Types { get; set; }
        [Required]
        public double Price { get; set; }

        public void Mapping(Profile profile) => profile.CreateMap<AddNewFoodToMenuCommand, Food>()
            .ForSourceMember(e => e.Price, opt => opt.DoNotValidate())
            .ForMember(e => e.Id, opt => opt.Ignore());
    }

    public sealed class AddNewFoodToMenuCommandHandler : IRequestHandler<AddNewFoodToMenuCommand, Response<MenuDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IUploadService _uploadService;

        public AddNewFoodToMenuCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IUploadService uploadService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _uploadService = uploadService;
        }

        public async Task<Response<MenuDto>> Handle(AddNewFoodToMenuCommand request, CancellationToken cancellationToken)
        {
            var menu = await _unitOfWork.MenuRepository.GetAsync(e => e.Id == request.Id && !e.IsDeleted);
            if (menu is null)
            {
                throw new NotFoundException(nameof(Menu), request.Id);
            }

            var food = _mapper.Map<Food>(request);
            var courseType = await _unitOfWork.CourseTypeRepository.GetAsync(e => e.Id == request.CourseTypeId && !e.IsDeleted);
            if (courseType is null)
            {
                throw new NotFoundException(nameof(Food.CourseType), request.CourseTypeId);
            }

            if (request.Types?.Any() == true)
            {
                foreach (var typeId in request.Types)
                {
                    var inDatabase = await _unitOfWork.TypeRepository.GetAsync(e => e.Id == typeId);
                    if (inDatabase is null)
                    {
                        throw new NotFoundException(nameof(Core.Entities.Type), typeId);
                    }
                    if (food.FoodTypes is null)
                    {
                        food.FoodTypes = new List<FoodType>();
                    }
                    food.FoodTypes.Add(new FoodType
                    {
                        Food = food,
                        TypeId = inDatabase.Id
                    });
                }

            }

            var pictureUrl = await _uploadService.UploadAsync(request.Picture, "Foods");
            food.PictureUrl = pictureUrl;
            //var insertedFood = await _unitOfWork.FoodRepository.InsertAsync(food);
            //await _unitOfWork.CompleteAsync(cancellationToken);
            //if (insertedFood is null)
            //{
            //    return new Response<MenuDto>("error");
            //}
            await _unitOfWork.MenuFoodRepository.InsertAsync(new MenuFood
            {
                Food = food,
                MenuId = menu.Id,
                Price = request.Price
            });

            await _unitOfWork.MenuRepository.UpdateAsync(menu);
            food.AddDomainEvent(new CreateFoodEvent()
            {
                Name = request.Name
            });
            menu.AddDomainEvent(new AddExistingFoodToMenuEvent
            {
                foodName = food.Name,
                menuName = menu.Name
            });
            await _unitOfWork.CompleteAsync(cancellationToken);
            return new Response<MenuDto>()
            {
                Succeeded = true,
                StatusCode = System.Net.HttpStatusCode.Created
            };
        }
    }
}