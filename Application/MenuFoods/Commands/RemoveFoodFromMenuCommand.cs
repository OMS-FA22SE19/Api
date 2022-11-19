using Application.Menus.Response;
using Application.Common.Models;
using AutoMapper;
using Core.Interfaces;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.MenuFoods.Commands
{
    public sealed class RemoveFoodFromMenuCommand : IRequest<Response<MenuDto>>
    {
        [JsonIgnore]
        public int MenuId { get; set; }
        [JsonIgnore]
        public int FoodId { get; set; }
    }

    public sealed class RemoveFoodFromMenuCommandHandler : IRequestHandler<RemoveFoodFromMenuCommand, Response<MenuDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RemoveFoodFromMenuCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<MenuDto>> Handle(RemoveFoodFromMenuCommand request, CancellationToken cancellationToken)
        {

            var result = await _unitOfWork.MenuFoodRepository.DeleteAsync(e => e.MenuId == request.MenuId && e.FoodId == request.FoodId);
            await _unitOfWork.CompleteAsync(cancellationToken);
            return new Response<MenuDto>()
            {
                Succeeded = true,
                StatusCode = System.Net.HttpStatusCode.NoContent
            };
        }
    }
}
