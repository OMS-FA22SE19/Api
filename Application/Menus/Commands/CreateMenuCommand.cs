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
    public sealed class CreateMenuCommand : IMapFrom<Menu>, IRequest<Response<MenuDto>>
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; }
        [StringLength(1000, MinimumLength = 2)]
        public string Description { get; set; }
        public bool IsHidden { get; set; } = true;

        public void Mapping(Profile profile)
        {
            profile.CreateMap<CreateMenuCommand, Menu>();
        }
    }

    public sealed class CreateMenuCommandHandler : IRequestHandler<CreateMenuCommand, Response<MenuDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateMenuCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<MenuDto>> Handle(CreateMenuCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Menu>(request);
            var result = await _unitOfWork.MenuRepository.InsertAsync(entity);
            await _unitOfWork.CompleteAsync(cancellationToken);
            if (result is null)
            {
                return new Response<MenuDto>("error");
            }
            var mappedResult = _mapper.Map<MenuDto>(result);
            return new Response<MenuDto>(mappedResult)
            {
                StatusCode = System.Net.HttpStatusCode.Created
            };
        }
    }
}
