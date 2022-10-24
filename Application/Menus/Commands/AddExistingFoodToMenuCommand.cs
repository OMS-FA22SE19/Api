using Application.Common.Exceptions;
using Application.Menus.Events;
using Application.Menus.Response;
using Application.Models;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Application.Menus.Commands
{
    public sealed class AddExistingFoodToMenuCommand : IRequest<Response<MenuDto>>
    {
        [JsonIgnore]
        public int Id { get; set; }
        [JsonIgnore]
        public int FoodId { get; set; }
        [Required]
        [Range(0, double.PositiveInfinity)]
        public double Price { get; set; }
    }

    public sealed class AddExistingFoodToMenuCommandHandler : IRequestHandler<AddExistingFoodToMenuCommand, Response<MenuDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AddExistingFoodToMenuCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<MenuDto>> Handle(AddExistingFoodToMenuCommand request, CancellationToken cancellationToken)
        {
            var menuInDatabase = await _unitOfWork.MenuRepository.GetAsync(e => e.Id == request.Id && !e.IsDeleted);
            if (menuInDatabase is null)
            {
                throw new NotFoundException(nameof(Menu), request.Id);
            }

            var menuMapped = _mapper.Map<Menu>(menuInDatabase);

            var foodInDatabase = await _unitOfWork.FoodRepository.GetAsync(e => e.Id == request.FoodId && !e.IsDeleted);
            if (foodInDatabase is null)
            {
                throw new NotFoundException(nameof(Food), request.Id);
            }

            var checkInDatabase = await _unitOfWork.MenuFoodRepository.GetAsync(e => e.FoodId == request.FoodId && e.MenuId == request.Id) != null;
            if (checkInDatabase)
            {
                return new Response<MenuDto>($"This menu {request.Id} has already included this food {request.FoodId}");
            }

            await _unitOfWork.MenuFoodRepository.InsertAsync(new MenuFood
            {
                FoodId = request.FoodId,
                MenuId = request.Id,
                Price = request.Price
            });

            await _unitOfWork.MenuRepository.UpdateAsync(menuInDatabase);

            menuInDatabase.AddDomainEvent(new AddExistingFoodToMenuEvent
            {
                foodName = foodInDatabase.Name,
                menuName = menuInDatabase.Name
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
