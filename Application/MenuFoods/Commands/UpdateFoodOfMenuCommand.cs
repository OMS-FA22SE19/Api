using Application.Menus.Response;
using Application.Models;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Application.MenuFoods.Commands
{
    public sealed class UpdateFoodOfMenuCommand : IRequest<Response<MenuDto>>
    {
        [JsonIgnore]
        public int Id { get; set; }
        [JsonIgnore]
        public int FoodId { get; set; }
        [Required]
        [Range(0, double.PositiveInfinity)]
        public double Price { get; set; }
    }

    public sealed class UpdateFoodOfMenuCommandHandler : IRequestHandler<UpdateFoodOfMenuCommand, Response<MenuDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateFoodOfMenuCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<MenuDto>> Handle(UpdateFoodOfMenuCommand request, CancellationToken cancellationToken)
        {
            var entity = await _unitOfWork.MenuFoodRepository.GetAsync(e => e.FoodId == request.FoodId && e.MenuId == request.Id);
            if (entity is null)
            {
                return new Response<MenuDto>($"This menu {request.Id} has not included this food {request.FoodId}");
            }

            MapToEntity(request, entity);

            await _unitOfWork.MenuFoodRepository.UpdateAsync(entity);
            await _unitOfWork.CompleteAsync(cancellationToken);
            return new Response<MenuDto>()
            {
                Succeeded = true,
                StatusCode = System.Net.HttpStatusCode.NoContent
            };
        }

        private void MapToEntity(UpdateFoodOfMenuCommand request, MenuFood? entity) => entity.Price = request.Price;
    }
}
