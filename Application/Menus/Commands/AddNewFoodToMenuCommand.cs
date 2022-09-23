using Application.Common.Exceptions;
using Application.Common.Mappings;
using Application.Menus.Response;
using Application.Models;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Menus.Commands
{
    public sealed class AddNewFoodToMenuCommand : IMapFrom<Food>, IRequest<Response<MenuDto>>
    {
        public int Id { get; set; }
        [Required]
        [StringLength(1000, MinimumLength = 5)]
        public string Name { get; set; }
        [Required]
        [StringLength(4000, MinimumLength = 5)]
        public string Description { get; set; }
        [Required]
        [StringLength(2000, MinimumLength = 5)]
        public string Ingredient { get; set; }
        public bool Available { get; set; } = true;
        [StringLength(2048, MinimumLength = 5)]
        public string PictureUrl { get; set; }
        [Required]
        [Range(0, double.PositiveInfinity)]
        public double Price { get; set; }

        public void Mapping(Profile profile) => profile.CreateMap<AddNewFoodToMenuCommand, Food>()
            .ForSourceMember(e => e.Price, opt => opt.DoNotValidate())
            .ForMember(e => e.Id, opt => opt.Ignore());
    }

    public sealed class AddNewFoodToMenuCommandHandler : IRequestHandler<AddNewFoodToMenuCommand, Response<MenuDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AddNewFoodToMenuCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<MenuDto>> Handle(AddNewFoodToMenuCommand request, CancellationToken cancellationToken)
        {
            var menu = await _unitOfWork.MenuRepository.GetAsync(e => e.Id == request.Id);
            if (menu is null)
            {
                throw new NotFoundException(nameof(Menu), request.Id);
            }

            var food = _mapper.Map<Food>(request);
            await _unitOfWork.MenuFoodRepository.InsertAsync(new MenuFood
            {
                Food = food,
                MenuId = menu.Id,
                Price = request.Price
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